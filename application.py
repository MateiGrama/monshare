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


@app.route("/login")
def login():
    username = request.args.get('username')
    password = request.args.get('password')
    result = {"text": "test request & response for" + username}
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
