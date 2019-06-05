import json

import pyodbc
from flask import Flask
from flask import request

from utils import keys
from utils.db_functionalities import Db
from utils.search_engine import levenshtein, Trie
from utils.utils import get_fields, error_status_response, SUCCESS_STATUS, logged_in, unauthorized_user, success_status, \
    get_random_ssid, group_list_to_json, messages_list_to_json, group_to_json, get_fields_in_dict

DEBUG = False
UPLOAD_FOLDER = '/home/site/wwwroot/uploads'

# The default value in kilometres used when the user searches for groups.
DEFAULT_RANGE = 2

app = Flask(__name__)

app.secret_key = keys.secret_key
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024
ALLOWED_EXTENSIONS = {'.apk'}

connection = pyodbc.connect(Db.get_connection_string(pyodbc.drivers()[-1]))
cursor = connection.cursor()
db = Db(cursor, connection)


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
    user_id, session_id, group_name, group_description, target_num, lifetime, lat, long, group_range = get_fields(
        'user_id', 'session_id', 'group_name', 'group_description', 'target', 'lifetime', 'lat', 'long', 'range')

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
            group_range if group_range else 'null'
        ))
        connection.commit()

        cursor.execute("select GroupId from groups order by CreationDateTime desc ")
        group_id = cursor.fetchone()

        cursor.execute("insert into UserToGroup (UserId, GroupId) values ({}, {})".format(user_id, group_id.GroupId))
        connection.commit()

        if result:
            result = {"status": SUCCESS_STATUS, "group_id": group_id.GroupId}
            return json.dumps(result)
    except Exception as e:
        return error_status_response("error while inserting group in db. Exception was: " + str(e))


@app.route("/updateGroup")
def update_group():
    user_id, session_id, group_id = get_fields('user_id', 'session_id', 'group_id')
    param_list = ['group_name', 'group_description', 'endDateTime', 'target', 'lat', 'long', 'range']
    param_dict = get_fields_in_dict(param_list)

    if not user_id or not session_id or not group_id:
        return error_status_response("invalid id or session id or group id")

    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        # Fails the user is not member of the group
        if not db.is_user_member_of_group(user_id, group_id):
            return error_status_response("User {} is not a member of group {}!".format(user_id, group_id))

        db.update_group([(param, val) for param, val in param_dict.items() if val and val != ""], group_id)

        row = db.get_group(group_id)
        columns = [column_description[0] for column_description in cursor.description]

        return group_to_json(row, columns)

    except Exception as e:
        return error_status_response("error while updating group. Exception was: " + str(e))


@app.route("/getGroups")
def get_groups():
    def get_filtered_list(l, edit_distance=2):
        return [k for k, v in l.items() if v <= edit_distance]

    def get_filtered_list2(l, keys):
        new_list = []

        for key in keys:
            for k in l:
                if k[1] == key:
                    new_list.append(k)
        return new_list

    user_id, session_id, lat, long, query, place_id = get_fields('user_id', 'session_id', 'lat',
                                                                 'long', 'query', 'place_id')

    if not user_id or not session_id or not lat or not long:
        return error_status_response("invalid id or session id or coordinates")

    try:
        if not logged_in(user_id, session_id):
            return unauthorized_user()

        if place_id is not None:
            # Search for a place
            rows = db.get_group_located_at(place_id)
        else:
            # Get groups around given position
            rows = db.get_groups_around(lat, long, DEFAULT_RANGE)

        columns = [column_description[0] for column_description in cursor.description]

        if len(rows) is 0:
            return group_list_to_json(rows, columns)

        # Search for a specific group
        if place_id is None:
            trie = Trie(item[1] for item in rows)
            suggestions = trie.get_auto_suggestions(query)
            rows = get_filtered_list2(rows, suggestions)

    except Exception as e:
        return error_status_response("error while getting groups. Exception was: " + str(e))

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

        rows = db.get_groups_of_user(user_id)
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

        if not db.group_exists(group_id):
            return error_status_response("No group with the given group id: {}.".format(group_id))
        db.add_user_to_group(user_id, group_id)
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
    if not db.is_user_member_of_group(user_id, group_id):
        return error_status_response("User {} is not a member of group {}!".format(user_id, group_id))

    # If the only member of the group leaves, delete the group
    if db.group_has_one_member(group_id):
        db.remove_user_from_group(user_id, group_id)
        db.delete_group(group_id)
        connection.commit()
        return success_status("You successfully left the group {} which has been deleted.".format(group_id))

    # If the owner leaves, pass the ownership to another user in that group.
    if db.is_group_owner(user_id, group_id):
        db.pass_ownership(user_id, group_id)

    db.remove_user_from_group(user_id, group_id)
    connection.commit()
    return success_status("You successfully left the group {}.".format(group_id))


@app.route("/deleteAccount")
def delete_account():
    user_id, session_id = get_fields('user_id', 'session_id')

    if not logged_in(user_id, session_id):
        return unauthorized_user()

    # Remove the user from all the groups
    groups = db.get_groups_of_user(user_id)
    for group in groups:
        leave_group(user_id, session_id, group[0])

    db.remove_user_from_database(user_id)
    return success_status("You successfully deleted your account!")


@app.route("/deleteGroup")
def delete_group_api():
    user_id, session_id, group_id = get_fields('user_id', 'session_id', 'group_id')
    return leave_group(user_id, session_id, group_id)


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
                          values ({},{},'{}',GETDATE())
                       """.format(group_id, user_id, message))
        connection.commit()

    except:
        return error_status_response("Error while processing sendMessage request.")
    return success_status("successfully sent a message.")
