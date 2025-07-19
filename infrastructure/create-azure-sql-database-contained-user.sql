CREATE USER [$(ManagedIdentityName)] FROM EXTERNAL PROVIDER;
GRANT CREATE SCHEMA TO [$(ManagedIdentityName)];
GRANT CREATE TABLE TO [$(ManagedIdentityName)];