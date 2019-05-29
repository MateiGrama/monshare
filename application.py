import json
import pyodbc
import os

from flask import Flask, flash, request, redirect, render_template
from werkzeug.utils import secure_filename
from flask import request

UPLOAD_FOLDER = '/home/site/wwwroot/uploads'

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

def allowed_file(filename):
	return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS
	

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
    username = request.args.get('username')
    firstname = request.args.get('firstname')
    lastname = request.args.get('lastname')
    return "register"


@app.route('/uploadAPK', methods=['POST'])
def upload_file():

    if(request.args.get('key')!= 'muieDragnea'):
        return "n-ai parola, nu pui sus."

	if request.method == 'POST':
        # check if the post request has the file part
		if 'file' not in request.files:
			flash('No file part')
			return redirect(request.url)
		file = request.files['file']
		if file.filename == '':
			flash('No file selected for uploading')
			return redirect(request.url)
		if file and allowed_file(file.filename):
			filename = secure_filename(file.filename)
			file.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
			flash('File(s) successfully uploaded')
			return redirect('/')
