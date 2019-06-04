from utils.keys import db_password
from utils.utils import DB_COL_NAME


def get_connection_string(driver):
    from application import DEBUG

    return "DRIVER={};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;" \
           "Uid=BoneyHadger@webapp-db-sv;Pwd={};Encrypt=yes;TrustServerCertificate=no;" \
           "Connection Timeout=30;".format(driver, db_password) if not DEBUG else \
        'DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=monshare-local-db;UID=user;PWD=admin1'


def pass_ownership(user_id, group_id):
    from application import cursor, connection
    # Get all users in the group, except the owner
    cursor.execute("""select UserToGroup.UserID
                      from Groups
                      join UserToGroup on Groups.GroupId = UserToGroup.GroupId
                      where Groups.GroupId = {} and UserToGroup.UserID != {}
                   """.format(group_id, user_id))
    new_owner = cursor.fetchone()[0]

    cursor.execute("update Groups set ownerId = {} where GroupId = {}".format(new_owner, group_id))
    connection.commit()


def is_user_member_of_group(user_id, group_id):
    from application import cursor
    cursor.execute(""" select Groups.GroupId
                       from (Groups
                       join UserToGroup on Groups.GroupId = UserToGroup.GroupId)
                       join Users on Users.UserId = UserToGroup.UserId
                       where Users.UserId = {} and Groups.GroupId = {}
                   """.format(user_id, group_id))
    return len(cursor.fetchall()) == 1


def group_has_one_member(group_id):
    from application import cursor
    cursor.execute(""" select count (*)
                       from (Users
                       join UserToGroup on Users.UserId = UserToGroup.UserId)
                       join Groups on Groups.GroupId = UserToGroup.GroupId
                       where Groups.GroupId = {}
                   """.format(group_id))
    return cursor.fetchone()[0] == 1


def update_group(params, group_id):
    from application import cursor, connection
    query = "update Groups set " + "', ".join([DB_COL_NAME[param] + " = '" + val for param, val in params]) + \
            "' where groupId = " + group_id
    cursor.execute(query)
    connection.commit()


def delete_group(group_id):
    from application import cursor, connection
    delete_group_messages(group_id)
    cursor.execute("delete from groups where GroupId = {}".format(group_id))
    connection.commit()


def is_group_owner(user_id, group_id):
    from application import cursor
    cursor.execute("select ownerId from Groups where GroupId = {}".format(group_id))
    return cursor.fetchone()[0] == int(user_id)


def remove_user_from_group(user_id, group_id):
    from application import cursor, connection
    cursor.execute("delete from UserToGroup where UserId = {} and GroupId = {} ".format(user_id, group_id))
    connection.commit()


def get_groups_of_user(user_id):
    from application import cursor
    cursor.execute(""" select *
                       from Groups
                       join UserToGroup on UserToGroup.GroupId = Groups.GroupId
                       where UserToGroup.UserId = {}
                   """.format(user_id))
    return cursor.fetchall()


def get_group(group_id):
    from application import cursor
    cursor.execute(""" select *
                       from Groups
                       where GroupId = {}
                   """.format(group_id))
    return cursor.fetchone()


def get_group_members(group_id):
    from application import cursor
    cursor.execute(""" select Users.UserId
                       from Users
                       join UserToGroup on UserToGroup.UserId = Users.UserId
                       where UserToGroup.GroupId = {}
                       """.format(group_id))


def remove_user_from_database(user_id):
    from application import cursor, connection
    cursor.execute("delete from Users where UserId = {}".format(user_id))
    connection.commit()


def delete_group_messages(group_id):
    from application import cursor, connection
    cursor.execute("delete from Group where groupId = {}".format(group_id))
    connection.commit()
