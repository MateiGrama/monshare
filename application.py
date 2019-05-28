import json
import pyodbc

from flask import Flask
from flask import request

app = Flask(__name__)

"""
#connectionString = "Driver={ODBC Driver 13 for SQL Server};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;Uid=BoneyHadger@webapp-db-sv;Pwd=HoneyBadger123$;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"
#cnxn = pyodbc.connect(connectionString)
"""
result = 0
try:
    drivers = [item for item in pyodbc.drivers()]
    driver = drivers[-1]
    result+= result
    print("driver:{}".format(driver))
    con_string = f'DRIVER={driver};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;Uid=BoneyHadger@webapp-db-sv;Pwd=HoneyBadger123$;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'
    print(con_string)
    result+= result
    cnxn = pyodbc.connect(con_string)
    cursor = cnxn.cursor()
    result+= result
except:
   result += 100     
    
@app.route("/")
def hello():
    try:
        cursor.execute("select * from Users")
        row = cursor.fetchone()
        return "Buenos dias, " +  str(row[1])
    except:
        return "nu a mers, dar ajunge aici: " + result


@app.route("/login")
def login():
    username = request.args.get('username')
    password = request.args.get('password')
    result= {"text": "test request & response for" + username}
    return json.dumps(result)


@app.route("/register")
def register():
    username = request.args.get('username')
    firstname = request.args.get('firstname')
    lastname = request.args.get('lastname')
    return json.dumps(result)

