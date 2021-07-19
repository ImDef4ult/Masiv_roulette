-- create database with name 'employeedb' before executing below  scripts


use roulettedb; 

CREATE TABLE `client` (
  `id_client` int NOT NULL AUTO_INCREMENT,
  `name` varchar(200) DEFAULT NULL,
  `password` varchar(20) DEFAULT NULL,
  `balance` float NOT NULL,
  PRIMARY KEY (`id_client`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `admin_users` (
  `id_admin` int NOT NULL AUTO_INCREMENT,
  `username` varchar(100) DEFAULT NULL,
  `password` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id_admin`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `roulette` (
  `id_roulette` int NOT NULL AUTO_INCREMENT,
  `roulette_name` varchar(100) DEFAULT NULL,
  `roulette_state` BOOLEAN,
  PRIMARY KEY (`id_roulette`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `bets` (
  `id_bet` int NOT NULL AUTO_INCREMENT,
  `id_client` int NOT NULL,
  `id_roulette` int NOT NULL,
  `bet_value` int NOT NULL,
  `bet_target_int` int DEFAULT 0,
  `bet_target_color` int DEFAULT 0,
  `gain` float DEFAULT 0,
  PRIMARY KEY (`id_bet`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO admin_users(username, password) VALUES ('ImDef4ult', '12345');

