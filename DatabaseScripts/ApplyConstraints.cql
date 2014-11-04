//Apply constraints (to be executed manually one at a time)
CREATE CONSTRAINT ON (s:Student) ASSERT s.StudentID IS UNIQUE
CREATE CONSTRAINT ON (u:User) ASSERT u.Username IS UNIQUE