/*
Navicat MySQL Data Transfer

Source Server         : Localhost
Source Server Version : 50616
Source Host           : localhost:3306
Source Database       : rotmg

Target Server Type    : MYSQL
Target Server Version : 50616
File Encoding         : 65001

Date: 2016-07-28 10:58:48
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for accounts
-- ----------------------------
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE `accounts` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `uuid` varchar(128) NOT NULL,
  `password` varchar(256) NOT NULL,
  `name` varchar(64) NOT NULL,
  `email` varchar(128) NOT NULL,
  `rank` int(11) NOT NULL,
  `tag` varchar(11) NOT NULL,
  `namechosen` tinyint(1) NOT NULL,
  `verified` tinyint(1) NOT NULL,
  `guild` int(11) NOT NULL,
  `guildRank` int(11) NOT NULL,
  `guildFame` int(11) NOT NULL DEFAULT '0',
  `vaultCount` int(11) NOT NULL,
  `maxCharSlot` int(11) NOT NULL,
  `regTime` datetime NOT NULL,
  `guest` tinyint(1) NOT NULL,
  `starred` text NOT NULL,
  `ignored` text NOT NULL,
  `banned` tinyint(1) NOT NULL,
  `beginnerPackageTimeLeft` int(11) NOT NULL DEFAULT '0',
  `locked` text NOT NULL,
  `muted` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`,`email`)
) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for boards
-- ----------------------------
DROP TABLE IF EXISTS `boards`;
CREATE TABLE `boards` (
  `guildId` int(11) NOT NULL,
  `text` varchar(1024) NOT NULL,
  PRIMARY KEY (`guildId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for characters
-- ----------------------------
DROP TABLE IF EXISTS `characters`;
CREATE TABLE `characters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `accId` int(11) NOT NULL,
  `charId` int(11) NOT NULL,
  `charType` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `exp` int(11) NOT NULL,
  `fame` int(11) NOT NULL,
  `items` text NOT NULL,
  `itemDatas` longtext NOT NULL,
  `backpacks` longtext NOT NULL,
  `hp` int(11) NOT NULL,
  `mp` int(11) NOT NULL,
  `stats` varchar(64) NOT NULL,
  `dead` tinyint(1) NOT NULL,
  `tex1` int(11) NOT NULL,
  `tex2` int(11) NOT NULL,
  `effect` longtext NOT NULL,
  `skin` int(11) NOT NULL DEFAULT '-1',
  `permaSkin` int(11) NOT NULL,
  `pet` int(11) NOT NULL,
  `xpboost` tinyint(1) NOT NULL,
  `nexus` text NOT NULL,
  `floors` int(11) NOT NULL,
  `fameStats` text NOT NULL,
  `createTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `deathTime` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  `totalFame` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=11 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for classstats
-- ----------------------------
DROP TABLE IF EXISTS `classstats`;
CREATE TABLE `classstats` (
  `accId` int(11) NOT NULL,
  `objType` int(11) NOT NULL,
  `bestLv` int(11) NOT NULL,
  `bestFame` int(11) NOT NULL,
  PRIMARY KEY (`accId`,`objType`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for death
-- ----------------------------
DROP TABLE IF EXISTS `death`;
CREATE TABLE `death` (
  `accId` int(11) NOT NULL,
  `chrId` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `charType` int(11) NOT NULL,
  `tex1` int(11) NOT NULL,
  `tex2` int(11) NOT NULL,
  `skin` int(11) NOT NULL DEFAULT '-1',
  `items` text NOT NULL,
  `itemDatas` longtext NOT NULL,
  `fame` int(11) NOT NULL,
  `fameStats` text NOT NULL,
  `totalFame` int(11) NOT NULL,
  `firstBorn` tinyint(1) NOT NULL,
  `killer` varchar(128) NOT NULL,
  `time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`accId`,`chrId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for emails
-- ----------------------------
DROP TABLE IF EXISTS `emails`;
CREATE TABLE `emails` (
  `accId` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(16) NOT NULL,
  `email` varchar(128) NOT NULL,
  `accessKey` varchar(40) DEFAULT NULL,
  PRIMARY KEY (`accId`),
  UNIQUE KEY `email` (`email`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for guilds
-- ----------------------------
DROP TABLE IF EXISTS `guilds`;
CREATE TABLE `guilds` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL DEFAULT 'DEFAULT_GUILD',
  `members` varchar(128) NOT NULL,
  `guildFame` int(11) NOT NULL,
  `totalGuildFame` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  PRIMARY KEY (`id`,`members`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for homes
-- ----------------------------
DROP TABLE IF EXISTS `homes`;
CREATE TABLE `homes` (
  `accId` int(11) NOT NULL,
  `data` text NOT NULL,
  PRIMARY KEY (`accId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for market
-- ----------------------------
DROP TABLE IF EXISTS `market`;
CREATE TABLE `market` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `accId` int(11) NOT NULL,
  `status` smallint(1) NOT NULL DEFAULT '0',
  `offerItems` text NOT NULL,
  `offerData` longtext NOT NULL,
  `requestItems` text NOT NULL,
  `requestData` longtext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for news
-- ----------------------------
DROP TABLE IF EXISTS `news`;
CREATE TABLE `news` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `icon` varchar(16) NOT NULL,
  `title` varchar(128) NOT NULL,
  `text` varchar(128) NOT NULL,
  `link` varchar(256) NOT NULL,
  `date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for sprites
-- ----------------------------
DROP TABLE IF EXISTS `sprites`;
CREATE TABLE `sprites` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `guid` varchar(128) NOT NULL,
  `name` varchar(256) NOT NULL,
  `dataType` int(11) NOT NULL,
  `tags` text NOT NULL,
  `data` longblob NOT NULL,
  `fileSize` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=17 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for stats
-- ----------------------------
DROP TABLE IF EXISTS `stats`;
CREATE TABLE `stats` (
  `accId` int(11) NOT NULL,
  `fame` int(11) NOT NULL,
  `totalFame` int(11) NOT NULL,
  `credits` int(11) NOT NULL,
  `totalCredits` int(11) NOT NULL,
  `souls` int(11) NOT NULL,
  `totalSouls` int(11) NOT NULL,
  PRIMARY KEY (`accId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for unlockedclasses
-- ----------------------------
DROP TABLE IF EXISTS `unlockedclasses`;
CREATE TABLE `unlockedclasses` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `accId` int(11) NOT NULL,
  `class` varchar(32) NOT NULL,
  `available` varchar(64) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=57 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for vaults
-- ----------------------------
DROP TABLE IF EXISTS `vaults`;
CREATE TABLE `vaults` (
  `accId` int(11) NOT NULL,
  `chestId` int(11) NOT NULL AUTO_INCREMENT,
  `items` text NOT NULL,
  `itemDatas` longtext NOT NULL,
  PRIMARY KEY (`accId`,`chestId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
