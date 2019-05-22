import json
from flask import Flask
from flask import request

app = Flask(__name__)

@app.route("/")
def hello():
    return "Buenos dias!"


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

