from utils.keys import db_password
from utils.utils import DB_COL_NAME


class Db:

    def __init__(self, cursor, connection):
        self.cursor = cursor
        self.connection = connection

    @staticmethod
    def get_connection_string(driver):
        from application import DEBUG

        return "DRIVER={};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;" \
               "Uid=BoneyHadger@webapp-db-sv;Pwd={};Encrypt=yes;TrustServerCertificate=no;" \
               "Connection Timeout=30;".format(driver, db_password) if not DEBUG else \
            'DRIVER={ODBC Driver 17 for SQL Server};SERVER=localhost;DATABASE=monshare-local-db;UID=user;PWD=admin1'

    def pass_ownership(self, user_id, group_id):
        # Get all users in the group, except the owner
        self.cursor.execute("""select UserToGroup.UserID
                               from Groups
                               join UserToGroup on Groups.GroupId = UserToGroup.GroupId
                               where Groups.GroupId = {} and UserToGroup.UserID != {}
                       """.format(group_id, user_id))
        new_owner = self.cursor.fetchone()[0]

        self.cursor.execute("update Groups set ownerId = {} where GroupId = {}".format(new_owner, group_id))
        self.connection.commit()

    def is_user_member_of_group(self, user_id, group_id):
        self.cursor.execute(""" select Groups.GroupId
                                from (Groups
                                join UserToGroup on Groups.GroupId = UserToGroup.GroupId)
                                join Users on Users.UserId = UserToGroup.UserId
                                where Users.UserId = {} and Groups.GroupId = {}
                       """.format(user_id, group_id))
        return len(self.cursor.fetchall()) == 1

    def group_has_one_member(self, group_id):
        self.cursor.execute(""" select count (*)
                                from (Users
                                join UserToGroup on Users.UserId = UserToGroup.UserId)
                                join Groups on Groups.GroupId = UserToGroup.GroupId
                                where Groups.GroupId = {}
                       """.format(group_id))
        return self.cursor.fetchone()[0] == 1

    def update_group(self, params, group_id):
        query = "update Groups set " + "', ".join([DB_COL_NAME[param] + " = '" + val for param, val in params]) + \
                "' where groupId = " + group_id
        self.cursor.execute(query)
        self.connection.commit()

    def delete_group(self, group_id):
        self.delete_group_messages(group_id)
        self.cursor.execute("delete from groups where GroupId = {}".format(group_id))
        self.connection.commit()

    def is_group_owner(self, user_id, group_id):
        self.cursor.execute("select ownerId from Groups where GroupId = {}".format(group_id))
        return self.cursor.fetchone()[0] == int(user_id)

    def remove_user_from_group(self, user_id, group_id):
        # Delete the user from the mapping from users to groups and update the number of members
        self.cursor.execute(""" begin transaction;
                                delete from UserToGroup where UserId = {0} and GroupId = {1};
                                update Groups set MembersNumber = MembersNumber - 1
                                where GroupId = {1};
                                commit;
                       """.format(user_id, group_id))
        self.connection.commit()

    def add_user_to_group(self, user_id, group_id):
        # Add the user to the mapping from users to groups and update the number of members
        self.cursor.execute(""" begin transaction;
                                insert into UserToGroup (userId, groupId) values ({0}, {1});
                                update Groups set MembersNumber = MembersNumber + 1
                                where GroupId = {1};
                                commit;
                       """.format(user_id, group_id))
        self.connection.commit()

    def get_groups_of_user(self, user_id):
        self.cursor.execute(""" select *
                                from Groups
                                join UserToGroup on UserToGroup.GroupId = Groups.GroupId
                                where UserToGroup.UserId = {}
                       """.format(user_id))
        return self.cursor.fetchall()

    def get_group(self, group_id):
        self.cursor.execute(" select * from Groups where GroupId = {}".format(group_id))
        return self.cursor.fetchone()

    def get_group_members(self, group_id):
        self.cursor.execute(""" select Users.UserId
                                from Users
                                join UserToGroup on UserToGroup.UserId = Users.UserId
                                where UserToGroup.GroupId = {}
                           """.format(group_id))

    def remove_user_from_database(self, user_id):
        self.cursor.execute("delete from Users where UserId = {}".format(user_id))
        self.connection.commit()

    def delete_group_messages(self, group_id):
        self.cursor.execute("delete from Messages where groupId = {}".format(group_id))
        self.connection.commit()

    def group_exists(self, group_id):
        self.cursor.execute("select groupId from groups where groupId={}".format(group_id))
        return self.cursor.fetchone()
