import json
from random import randint

import pyodbc
from flask import Flask
from flask import request

from utils import keys, db_functionalities
from utils.db_functionalities import is_user_member_of_group, group_has_one_member, delete_group, is_group_owner, \
    pass_ownership, remove_user_from_group
from utils.utils import get_fields

DEBUG = False
UPLOAD_FOLDER = '/home/site/wwwroot/uploads'
FAIL_STATUS = "fail"
SUCCESS_STATUS = "success"

app = Flask(__name__)

app.secret_key = keys.secret_key
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024
ALLOWED_EXTENSIONS = {'.apk'}

connection = pyodbc.connect(db_functionalities.get_connection_string(pyodbc.drivers()[-1]))
cursor = connection.cursor()


@app.route("/")
def welcome_page():
    return "Welcome to MonShare"


@app.route("/login")
def login():
    email = request.args.get('email')
    password_hash = request.args.get('password_hash')

    if not email or not password_hash:
        return error_status_response("No email or password provided.")

    # Check that user is in database
    try:
        cursor.execute("SELECT SessionId, PasswordHash FROM users WHERE email = '{}';".format(email));
        user_details = cursor.fetchone()

        if not user_details:
            return error_status_response("No user registered with the given email address.")
        if not user_details.PasswordHash == password_hash:
            return error_status_response("Wrong password provided.")

        cursor.execute(
            "UPDATE users SET sessionId = {} WHERE email = '{}'".format(int(user_details.SessionId) + 1, email))
        connection.commit()

        # Return success result
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email))
        user_details = cursor.fetchone()

        result = {"status": SUCCESS_STATUS, "user": {"user_id": user_details.UserId,
                                                     "session_id": user_details.SessionId,
                                                     "first_name": user_details.FirstName,
                                                     "last_name": user_details.LastName}}

    except:
        return error_status_response("error while processing login request")
    return json.dumps(result)


@app.route("/logout")
def logout():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')

    if not user_id or not session_id:
        return error_status_response("No user_id or session_id provided.")

    # Check that the connection is valid
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        cursor.execute("UPDATE users SET sessionId = {} WHERE userId = {}".format(getRandomSSID(), user_id))
        connection.commit()

    except:
        return error_status_response("error while processing login request")
    return success_status()


@app.route("/register")
def register():
    email = request.args.get('email')
    first_name = request.args.get('first_name')
    last_name  = request.args.get('last_name')
    password_hash  = request.args.get('password_hash')

    if not email or not password_hash or not first_name or not last_name:
        return error_status_response("Provided input is not valid.")
    line = 0
    # Check that user is in database
    try:
        line += 1
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email));
        if len(cursor.fetchall()) > 0:
            return error_status_response("Email already in use.")
        line += 1

        cursor.execute("INSERT INTO users (firstname, lastname, passwordhash, sessionId, email) values ('{}','{}','{}','{}','{}')".format(first_name, last_name, password_hash, getRandomSSID(), email))
        connection.commit()
        line += 1

        # Return success result
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email))
        line += 1
        user_details = cursor.fetchone()
        line += 1

        result = {"status": SUCCESS_STATUS, "user": {"user_id": user_details.UserId,
                                                     "session_id": user_details.SessionId,
                                                     "first_name": user_details.FirstName,
                                                     "last_name": user_details.LastName}}
        line += 1


    except:
        return error_status_response("error while processing register request, line:" + str(line))
    return json.dumps(result)


@app.route("/createGroup")
def createGroup():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')
    group_name = request.args.get('group_name')
    group_description = request.args.get('group_description')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")

    try:
        if logged_in(user_id, session_id):
            return unauthorized_user()

        result = cursor.execute(
            """insert into groups (title, description, creationdatetime, ownerId)
               values ('{}','{}',GETDATE(), {})""".format(group_name, group_description, user_id))
        connection.commit()

        if result:
            return success_status("Successfully created group!")

    except:
        return error_status_response("error while inserting group in db")


@app.route("/getGroupsAround")
def getGroupsAround():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()
        cursor.execute("select * from groups")
        columns = [column_description[0] for column_description in cursor.description]
        rows = cursor.fetchall()
    except:
        return error_status_response("error while getting all groups")

    return group_list_to_json(rows, columns)


@app.route("/joinGroup")
def joinGroup():
    pass


@app.route("/leaveGroup")
def leave_group(*args):
    if len(args) is not 0 and len(args) is not 3:
        return error_status_response("Incorrect number of arguments."
                                     "Expected 0 or 3, but found {}. {}".format(len(args), args))

    if len(args) is 0:
        user_id, session_id, group_id = get_fields('user_id', 'session_id', 'group_id')
    else:
        user_id, session_id, group_id = args

    if not logged_in(user_id, session_id):
        return unauthorized_user()

    # Fails the user is not member of the group
    if not is_user_member_of_group(user_id, group_id):
        return error_status_response("User {} is not a member of group {}!".format(user_id, group_id))

    # If the only member of the group leaves, delete the group
    if group_has_one_member(group_id):
        remove_user_from_group(user_id, group_id)
        delete_group(group_id)
        connection.commit()
        return success_status("You successfully left the group {} which now has been deleted.".format(group_id))

    # If the owner leaves, pass the ownership to another user in that group.
    if is_group_owner(user_id, group_id):
        pass_ownership(user_id, group_id)

    remove_user_from_group(user_id, group_id)
    connection.commit()
    return success_status("You successfully left the group {}.".format(group_id))


@app.route("/deleteAccount")
def delete_account():
    pass


def group_list_to_json(rows, columns):
    try:
        groups = []
        for row in rows:
            if not row[3] is None:
                row[3] = row[3].strftime('%Y-%m-%dT%H:%M:%S.%f')
            if not row[4] is None:
                row[4] = row[4].strftime('%Y-%m-%dT%H:%M:%S.%f')
            groups.append(dict(zip(columns, row)))
        result = {'status': 'success', 'groups': groups}
        return json.dumps(result)
    except:
        return error_status_response("error while generating json for rows")


def logged_in(user_id, session_id):
    cursor.execute("select * from Users where userid={} and sessionid={}".format(user_id, session_id))
    return cursor.fetchall()


def unauthorized_user():
    return error_status_response("user has not got a valid session id")


def success_status(msg):
    result = {"message": msg, "status": SUCCESS_STATUS}
    return json.dumps(result)


def error_status_response(msg):
    result = {"description": msg, "status": FAIL_STATUS}
    return json.dumps(result)

def getRandomSSID():
    return randint(1,100000000);
