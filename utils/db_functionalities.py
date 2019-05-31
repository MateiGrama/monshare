from utils.keys import db_password


def get_connection_string(driver):
    return "DRIVER={};Server=tcp:webapp-db-sv.database.windows.net,1433;Database=WebAppDb;" \
           "Uid=BoneyHadger@webapp-db-sv;Pwd={};Encrypt=yes;TrustServerCertificate=no;" \
           "Connection Timeout=30;".format(driver, db_password)
