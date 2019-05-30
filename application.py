import json
import pyodbc
import os

from flask import Flask, flash, request, redirect, render_template
from werkzeug.utils import secure_filename
from flask import request

UPLOAD_FOLDER = '/home/site/wwwroot/uploads'
FAIL_STATUS = "fail"
SUCCESS_STATUS = "success"

app = Flask(__name__)

app.secret_key = "secret key"
app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024
ALLOWED_EXTENSIONS = set(['.apk'])

drivers = [item for item in pyodbc.drivers()]
driver = drivers[-1]
con_string = "DRIVER={};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;Uid=BoneyHadger@webapp-db-sv;Pwd=HoneyBadger123$;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;".format(driver)
print(con_string)
cnxn = pyodbc.connect(con_string)
cursor = cnxn.cursor()


@app.route("/")
def hello():
    try:
        cursor.execute("select * from Users")
        row = cursor.fetchone()
        return "Muie Microsoft, " +  str(row[1]) + 'os'
    except:
        return "nu a mers, but routed correctly to hello."


@app.route("/login")
def login():
    username = request.args.get('username')
    password = request.args.get('password')
    result= {"text": "test request & response for" + username}
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
    
    line = 0
    try:
        if check_login(user_id, session_id):
            return authentification_failed()
        
        line += 1
        result = cursor.execute("insert into groups (title, description,creationdatetime, ownerId) values ('{}','{}',GETDATE(), {})".format(group_name, group_description ,user_id))
        cnxn.commit()
        if result:
            return success_status()

    except:
        return error_status_response("error while inserting group in db; line: " + str(line))


    
@app.route("/getGroupsAround")
def getGroupsAround():
    user_id = request.args.get('user_id')
    session_id = request.args.get('session_id')

    if not user_id or not session_id:
        return error_status_response("invalid id or sessionid")
    line = 0
    try:
       if check_login(user_id, session_id):
           return authentification_failed()
       line += 1
       cursor.execute("select * from groups")
       line += 1       
       columns = [column_description[0] for column_description in cursor.description]
       rows = cursor.fetchall()
       line += 1       
       return group_list_to_json(rows, columns)
       
    except:
        return error_status_response("error while getting all groups; line:" + str(line))
        

@app.route("/joinGroup")
def joinGroup():
    pass 
@app.route("/leaveGroup")
def leaveGroup():
    pass

def group_list_to_json(rows, columns):
    line += 1
    groups = []
    for row in rows:
        groups.append(dict(zip(columns, row)))
    line += 1
    result = {'status':'success' , 'groups' : json.dumps(groups)}
    line += 1
    return json.dumps(result)


def check_login(user_id, session_id):
    cursor.execute("select * from Users where userid={} and sessionid={}".format(user_id, session_id))
    return not cursor.fetchall()

def authentification_failed():
    return error_status_response("user has not got a valid session id")

def success_status():
   result = {"status" : SUCCESS_STATUS}
   return json.dumps(result)

def error_status_response(msg):
   result = { "message" : msg , "status" : FAIL_STATUS}
   return json.dumps(result)
