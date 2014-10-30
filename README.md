DreamScheduler
==============

SOEN 341 project - DreamTeam 

----------------------------------------------------------
- this is the main repo for the application
- everyone must fork (copy) to his own account
- when you create a new feature -> pull request -> we can then check it and add it to the main one

-----------------------------------------------------------
### current app

- install neo4j 
- start the database 
- your database port must match this "http://localhost:7474/db/data" `if not you will need to update the code in the controller`

- can create account (member), admin only by using the neo4j web browser 
- log into the system
  - validation for input login/registration 
  - authentification method during login process
- webapp is divide into 3 sub system 
  - public, allow non logged in user to see
    - see basic home page that descripe app
    - can register, log in
  - member which are the student
    - see academic record
    - see course list
    - see professor list
    - sequence
  - admin 
   - database page which offer a button that will later trigger a database update process
  - the 3 different user see different navigation tab, view different page
    - they also can't see function from other sub system ( security prevent this)
- user can see their account information
  - they can modfiy specific data like first name last name
