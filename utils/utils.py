from random import randint
from flask import request, json

FAIL_STATUS = "fail"
SUCCESS_STATUS = "success"


def get_fields(*args):
    fields = []
    for arg in args:
        fields.append(request.args.get(arg))
    return fields


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
    from application import cursor

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


def get_random_SSID():
    return randint(1, 100000000)
