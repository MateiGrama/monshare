import json
import pyodbc

from flask import Flask, flash, request, redirect, render_template
from flask import request
from utils import keys
from utils import db_functionalities

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

    line = 0

    if not email or not password_hash:
        return error_status_response("No email or password provided.")

    # Check that user is in database
    try:
        cursor.execute("SELECT SessionId, PasswordHash FROM users WHERE email = '{}';".format(email));
        user_details = cursor.fetchone()
        line += 1
        if not user_details:
            return error_status_response("No user registered with the given email address.")
        line += 1
        if not user_details.PasswordHash == password_hash:
            return error_status_response("Wrong password provided.")
        line += 1
        cursor.execute("UPDATE users SET sessionId = {} WHERE email = '{}'".format(int(user_details.SessionId) + 1, email))
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
        return error_status_response("error while processing login request" + str(line))
    return json.dumps(result)


@app.route("/register")
def register():
    email = request.args.get('email')
    firstname = request.args.get('firstname')
    lastname = request.args.get('lastname')
    return "register"


@app.route("/createGroup")
def createGroup():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')
    group_name = request.args.get('group_name')
    group_description = request.args.get('group_description')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")

    try:
        if check_login(user_id, session_id):
            return authentification_failed()

        result = cursor.execute(
            """insert into groups (title, description, creationdatetime, ownerId)
               values ('{}','{}',GETDATE(), {})""".format(group_name, group_description, user_id))
        connection.commit()

        if result:
            return success_status()

    except:
        return error_status_response("error while inserting group in db")



@app.route("/getGroupsAround")
def getGroupsAround():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")
    try:
        if check_login(user_id, session_id):
            return authentification_failed()
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
def leaveGroup():
    pass


def group_list_to_json(rows, columns):
    try:
        groups = []
        for row in rows:
            if (not row[3] is None):
                row[3] = row[3].strftime('%Y-%m-%dT%H:%M:%S.%f')
            if (not row[4] is None):
                row[4] = row[4].strftime('%Y-%m-%dT%H:%M:%S.%f')
            groups.append(dict(zip(columns, row)))
        result = {'status': 'success', 'groups': groups}
        return json.dumps(result)
    except:
        return error_status_response("error while generating json for rows")


def check_login(user_id, session_id):
    cursor.execute("select * from Users where userid={} and sessionid={}".format(user_id, session_id))
    return not cursor.fetchall()


def authentification_failed():
    return error_status_response("user has not got a valid session id")


def success_status():
    result = {"status": SUCCESS_STATUS}
    return json.dumps(result)


def error_status_response(msg):
    result = {"message": msg, "status": FAIL_STATUS}
    return json.dumps(result)
