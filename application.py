import json

import pyodbc
from flask import Flask
from flask import request

from utils import keys, db_functionalities
from utils.db_functionalities import is_user_member_of_group, group_has_one_member, delete_group, is_group_owner, \
    pass_ownership, remove_user_from_group, get_groups_of_user, remove_user_from_database
from utils.utils import get_fields, error_status_response, SUCCESS_STATUS, logged_in, unauthorized_user, success_status, \
    get_random_ssid, group_list_to_json, messages_list_to_json

DEBUG = True
UPLOAD_FOLDER = '/home/site/wwwroot/uploads'

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


@app.route("/register")
def register():
    email, first_name, last_name, password_hash = get_fields('email', 'first_name', 'last_name', 'password_hash')

    if not email or not password_hash or not first_name or not last_name:
        return error_status_response("Provided input is not valid.")

    # Check that user is in database
    try:
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email))
        if len(cursor.fetchall()) > 0:
            return error_status_response("Email already in use.")

        cursor.execute("""INSERT INTO users (firstname, lastname, passwordhash, sessionId, email)
                          values ('{}','{}','{}','{}','{}')
                       """.format(first_name, last_name, password_hash, get_random_ssid(), email))
        connection.commit()

        # Return success result
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email))
        user_details = cursor.fetchone()

        result = {"status": SUCCESS_STATUS, "user": {"user_id": user_details.UserId,
                                                     "session_id": user_details.SessionId,
                                                     "first_name": user_details.FirstName,
                                                     "last_name": user_details.LastName}}

    except:
        return error_status_response("Error while processing register request.")
    return json.dumps(result)


@app.route("/login")
def login():
    email, password_hash = get_fields('email', 'password_hash')

    if not (email and password_hash):
        return error_status_response("No email or password provided.")

    # Check that user is in database
    try:
        cursor.execute("SELECT SessionId, PasswordHash FROM users WHERE email = '{}';".format(email))
        user_details = cursor.fetchone()

        if not user_details:
            return error_status_response("No user registered with the given email address.")
        if not user_details.PasswordHash == password_hash:
            return error_status_response("Wrong password provided.")

        cursor.execute("""UPDATE users SET sessionId = {}
                          WHERE email = '{}'
                       """.format(int(user_details.SessionId) + 1, email))
        connection.commit()

        # Return success result
        cursor.execute("SELECT * FROM users WHERE email = '{}';".format(email))
        user_details = cursor.fetchone()

        result = {"status": SUCCESS_STATUS, "user": {"user_id": user_details.UserId,
                                                     "session_id": user_details.SessionId,
                                                     "first_name": user_details.FirstName,
                                                     "last_name": user_details.LastName}}

    except:
        return error_status_response("Error while processing login request.")
    return json.dumps(result)


@app.route("/isLoggedIn")
def is_logged_in():
    user_id, session_id = get_fields('user_id', 'session_id')

    if not (user_id and session_id):
        return error_status_response("No user_id or session_id provided.")

    # Check that the connection is valid
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        return success_status("User is already logged in.")
    except:
        return error_status_response("Something went wrong when queering database about user's login status.")


@app.route("/logout")
def logout():
    user_id, session_id = get_fields('user_id', 'session_id')

    if not (user_id and session_id):
        return error_status_response("No user_id or session_id provided.")

    # Check that the connection is valid
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        cursor.execute("UPDATE users SET sessionId = {} WHERE userId = {}".format(get_random_ssid(), user_id))
        connection.commit()

    except:
        return error_status_response("Error while processing logout request.")
    return success_status("Logout succeded.")


@app.route("/createGroup")
def create_group():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')
    group_name = request.args.get('group_name')
    group_description = request.args.get('group_description')
    target_num = request.args.get('target')
    lifetime = request.args.get('lifetime')
    lat = request.args.get('lat')
    long = request.args.get('long')
    range = request.args.get('range')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")

    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        result = cursor.execute("""insert into groups
                (title, description, creationdatetime, enddatetime, ownerid, lat, long, membersnumber, targetnum, groupRange)
                values ('{}','{}',GETDATE(), {}, {}, {}, {}, {}, {}, {})""".format(
            group_name,
            group_description,
            'DATEADD(minute, {}, GETDATE())'.format(lifetime) if lifetime else 'null',
            user_id,
            lat if lat else 'null',
            long if long else 'null',
            1,
            target_num if target_num else 'null',
            range if range else 'null'
        ))
        connection.commit()

        cursor.execute("select GroupId from groups order by CreationDateTime desc")
        group_id = cursor.fetchone()

        cursor.execute("insert into UserToGroup (UserId, GroupId) values ({}, {})".format(user_id, group_id.GroupId))
        connection.commit()

        if result:
            result = {"status": SUCCESS_STATUS, "group_id": group_id.GroupId}
            return json.dumps(result)
    except Exception as e:
        return error_status_response("error while inserting group in db. Exception was: " + str(e))


@app.route("/getGroupsAround")
def get_groups_around():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')

    if not (user_id and session_id):
        return error_status_response("invalid id or session id")
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()
        cursor.execute("select * from groups")
        columns = [column_description[0] for column_description in cursor.description]
        rows = cursor.fetchall()
    except:
        return error_status_response("error while getting all groups")

    return group_list_to_json(rows, columns)


@app.route("/getMyGroups")
def get_my_groups():
    user_id, session_id = get_fields('user_id', 'session_id')

    if not (user_id and session_id):
        return error_status_response("invalid id or session id")

    try:
        try:
            if not logged_in(user_id, session_id):
                return unauthorized_user()
        except pyodbc.Error as err:
            return error_status_response(err)

        rows = get_groups_of_user(user_id)
        columns = [column_description[0] for column_description in cursor.description]
    except pyodbc.Error as err:
        return error_status_response("Error while getting your groups! {}".format(err))

    return group_list_to_json(rows, columns)


@app.route("/joinGroup")
def join_group():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')
    group_id = request.args.get('group_id')
    try:
        if not user_id or not session_id:
            return error_status_response("invalid id or sessionid")
        if not group_id:
            return error_status_response("No group id provided.")
        if not logged_in(user_id, session_id):
            return unauthorized_user()
        cursor.execute("""select groupId from groups where groupId={};""".format(group_id))
        if not cursor.fetchone():
            return error_status_response("No group with the given group id.")
        cursor.execute("""insert into userToGroup (userId, groupId) values ({}, {})""".format(user_id, group_id))
        connection.commit()
    except:
        return error_status_response("error while getting all groups.")

    return success_status("User successfully added to the group.")


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
    user_id, session_id = get_fields('user_id', 'session_id')

    if not logged_in(user_id, session_id):
        return unauthorized_user()

    # Remove the user from all the groups
    groups = get_groups_of_user(user_id)
    for group in groups:
        leave_group(user_id, session_id, group[0])

    remove_user_from_database(user_id)
    return success_status("You successfully deleted your account!")


@app.route("/deleteGroup")
def delete_group_api():
    leave_group()


@app.route("/getGroupChat")
def get_group_chat():
    user_id, session_id, group_id = get_fields('user_id', 'session_id', 'group_id')

    if not (user_id and session_id):
        return error_status_response("No user_id or session_id provided.")

    # Check that the connection is valid
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()
        cursor.execute("""select senderid as msg_sender_id,  message as msg , datetime as date_time from messages 
                          where groupId={}
                       """.format(group_id))
        columns = [column_description[0] for column_description in cursor.description]
        rows = cursor.fetchall()

    except:
        return error_status_response("Error while processing group message request.")

    return messages_list_to_json(rows, columns)


@app.route("/sendMessage")
def send_message():
    user_id, session_id, group_id, message = get_fields('user_id', 'session_id', 'group_id', 'message')

    if not (user_id and session_id):
        return error_status_response("No user_id or session_id provided.")

    # Check that the connection is valid
    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        cursor.execute("""insert into messages (groupid, senderid, message, datetime) 
                          values ({},{},{},GETDATE())
                       """.format(group_id, user_id, message))
        connection.commit()

    except:
        return error_status_response("Error while processing logout request.")
    return success_status("successfully sent a message.")
