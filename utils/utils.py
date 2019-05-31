from flask import request


def get_fields(*args):
    fields = []
    for arg in args:
        fields.append(request.args.get(arg))
    return fields
