CREATE DATABASE uniusers /*!40100 DEFAULT CHARACTER SET utf8mb4 */;

-- uniusers.credentials definition
CREATE TABLE credentials (
  id int(11) NOT NULL AUTO_INCREMENT,
  created timestamp NOT NULL DEFAULT current_timestamp(),
  lastmodify timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  username varchar(50) DEFAULT NULL,
  password varchar(100) DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- uniusers.logaccess definition
CREATE TABLE logaccess (
  id int(11) NOT NULL AUTO_INCREMENT,
  created timestamp NOT NULL DEFAULT current_timestamp(),
  username varchar(50) DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- uniusers.roles definition
CREATE TABLE roles (
  id int(11) NOT NULL DEFAULT 0,
  created timestamp NOT NULL DEFAULT current_timestamp(),
  lastmodify timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  name varchar(25) DEFAULT NULL,
  PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- uniusers.userroles definition
CREATE TABLE userroles (
  id int(11) NOT NULL AUTO_INCREMENT,
  created timestamp NOT NULL DEFAULT current_timestamp(),
  lastmodify timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  userid int(11) DEFAULT NULL,
  roleid int(11) DEFAULT NULL,
  PRIMARY KEY (id),
  KEY userroles_FK (userid),
  CONSTRAINT userroles_FK FOREIGN KEY (userid) REFERENCES users (id) ON DELETE CASCADE
  CONSTRAINT userroles_FK_1 FOREIGN KEY (roleid) REFERENCES roles (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- uniusers.users definition
CREATE TABLE users (
  id int(11) NOT NULL AUTO_INCREMENT,
  created timestamp NOT NULL DEFAULT current_timestamp(),
  firstname varchar(50) DEFAULT NULL,
  lastname varchar(50) DEFAULT NULL,
  dateofbirth datetime DEFAULT NULL,
  phonenumber varchar(50) DEFAULT NULL,
  address varchar(200) DEFAULT NULL,
  avatar blob DEFAULT NULL,
  PRIMARY KEY (id),
  CONSTRAINT users_FK FOREIGN KEY (id) REFERENCES credentials (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- uniusers.usersview source
CREATE OR REPLACE
ALGORITHM = UNDEFINED VIEW uniusers.usersview AS
SELECT
    u.id AS id,
    u.created AS created,
    u.firstname AS firstname,
    u.lastname AS lastname,
    u.dateofbirth AS dateofbirth,
    u.phonenumber AS phonenumber,
    u.address AS address,
    u.avatar AS avatar,
    c.username AS email
FROM
    (uniusers.users u
JOIN uniusers.credentials c ON
    (c.id = u.id));

CREATE USER 'uniusers'@'%' IDENTIFIED BY 'R!3-s&4Tj=maS3!y';
GRANT ALL PRIVILEGES ON uniusers.credentials TO  'uniusers'@'%';
GRANT ALL PRIVILEGES ON uniusers.logaccess TO  'uniusers'@'%';
GRANT ALL PRIVILEGES ON uniusers.roles TO  'uniusers'@'%';
GRANT ALL PRIVILEGES ON uniusers.userroles TO  'uniusers'@'%';
GRANT ALL PRIVILEGES ON uniusers.users TO  'uniusers'@'%';
GRANT ALL PRIVILEGES ON uniusers.usersview  TO  'uniusers'@'%';