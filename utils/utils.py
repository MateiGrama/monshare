from random import randint
from flask import request, json

FAIL_STATUS = "fail"
SUCCESS_STATUS = "success"

DB_COL_NAME = {
    'user_id': 'userId',
    'session_id': 'sessionId',
    'group_name': 'title',
    'group_description': 'description',
    'target': 'targetNum',
    'endDateTime': 'endDateTime',
    'lat': 'lat',
    'long': 'long',
    'range': 'groupRange'
}


def get_fields(*args):
    fields = []
    for arg in args:
        fields.append(request.args.get(arg))
    return fields


def get_fields_in_dict(args):
    fields = []
    for arg in args:
        fields.append((arg, request.args.get(arg)))
    return dict(fields)


def group_list_to_json(rows, columns):
    try:
        groups = []
        for row in rows:
            row = list(row)
            if len(row) > 5:
                if not row[3] is None:
                    row[3] = row[3].strftime('%Y-%m-%dT%H:%M:%S.%f')
                if not row[4] is None:
                    row[4] = row[4].strftime('%Y-%m-%dT%H:%M:%S.%f')
            groups.append(dict(zip(columns, tuple(row))))
        result = {'status': 'success', 'groups': groups}
        return json.dumps(result)
    except:
        return error_status_response("error while generating json for rows")


def group_to_json(row, columns):
    try:
        if len(row) > 5:
            if not row[3] is None:
                row[3] = row[3].strftime('%Y-%m-%dT%H:%M:%S.%f')
            if not row[4] is None:
                row[4] = row[4].strftime('%Y-%m-%dT%H:%M:%S.%f')
        result = {'status': 'success', 'group': dict(zip(columns, row))}
        return json.dumps(result)
    except:
        return error_status_response("error while generating json for given group")


def messages_list_to_json(rows, columns):
    try:
        groups = []
        for row in rows:
            groups.append(dict(zip(columns, row)))
        result = {'status': 'success', 'messages': groups}
        return json.dumps(result)
    except:
        return error_status_response("error while generating json for rows")


def logged_in(user_id, session_id):
    from application import cursor
    cursor.execute("select * from Users where userid={} and sessionid={}".format(user_id, session_id))
    return cursor.fetchall()


def unauthorized_user():
    return error_status_response("user has not got a valid session id")


def success_status():
    return success_status("successfuly done.")


def success_status(msg):
    result = {"message": msg, "status": SUCCESS_STATUS}
    return json.dumps(result)


def error_status_response(msg):
    result = {"description": msg, "status": FAIL_STATUS}
    return json.dumps(result)


def get_random_ssid():
    return randint(1, 100000000)


def levenshtein(s1, s2):
    import numpy as np

    size_x = len(s1) + 1
    size_y = len(s2) + 1
    matrix = np.zeros((size_x, size_y))
    for x in range(size_x):
        matrix[x, 0] = x
    for y in range(size_y):
        matrix[0, y] = y

    for x in range(1, size_x):
        for y in range(1, size_y):
            if s1[x - 1] == s2[y - 1]:
                matrix[x, y] = min(matrix[x - 1, y] + 1, matrix[x - 1, y - 1], matrix[x, y - 1] + 1)
            else:
                matrix[x, y] = min(matrix[x - 1, y] + 1, matrix[x - 1, y - 1] + 1, matrix[x, y - 1] + 1)
    return matrix[size_x - 1, size_y - 1]
