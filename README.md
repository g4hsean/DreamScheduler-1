DreamScheduler
==============

#### TESTING SERVER 
- dream.michalwozniak.ca (live)  

### Description

- System designed to automate readjustments to the departmental sequences and optimize the resulting sequences to meet any special needs of the user. 


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

### Directory 
- DatabaseScripts 
  - ApplyConstraints.cql  -> All queries must be performed on the neo4j database using the browser 
- WebScrapper
  - Contains all the necessary python script
  - Specific Readme to understand python execution 
- DreamSchedulerApplication
  - Contains the application 
