drop procedure if exists AllICX;
DELIMITER //
CREATE PROCEDURE AllICX()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idpartner,partnerName from partner
where partnerType=2
order by partnerName;

END //
DELIMITER ;


drop procedure if exists AllAns;
DELIMITER //
CREATE PROCEDURE AllAns()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idpartner,partnerName from partner
where partnerType=1
order by partnerName;

END //
DELIMITER ;


drop procedure if exists AllIntlPartners;
DELIMITER //
CREATE PROCEDURE AllIntlPartners()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idpartner,partnerName from partner
where partnerType=3
order by partnerName;

END //
DELIMITER ;


drop procedure if exists RecentCalls;
DELIMITER //
CREATE PROCEDURE RecentCalls()
BEGIN


select DATE_FORMAT(`starttime`,'%d/%m/%Y %T') as `Start Time`,DATE_FORMAT(`answertime`,'%d/%m/%Y %T') as `Answer Time`,DATE_FORMAT(`endtime`,'%d/%m/%Y %T') as `End Time`,
cd.Type,cr.partnerName as 'Originating partner',r.description as 'Originating Route',
cr1.partnerName as 'Terminating partner',r1.description as 'Terminating Route',
TerminatingCalledNumber as 'Called Number',TerminatingCallingNumber as 'Calling Number',durationsec as 'Duration in Seconds'
from cdr c
left join Enumservicegroup cd
on c.servicegroup=cd.id
left join Route r
on c.incomingroute=r.routename
and c.inpartnerid=r.idpartner
and c.switchid=r.switchid
left join partner cr
on r.idpartner=cr.idpartner
left join Route r1
on c.outgoingroute=r1.routename
and c.outpartnerid=r1.idpartner
and c.switchid=r1.switchid
left join partner cr1
on r1.idpartner=cr1.idpartner
where chargingstatus=1
order by starttime desc
limit 0,100;

END //
DELIMITER ;


drop procedure if exists RecentCallsSummary;
DELIMITER //
CREATE PROCEDURE RecentCallsSummary(IN p_StartDateTime varchar(45), IN p_EndDateTime varchar(45))
BEGIN

declare varStartDate varchar(45);
declare varEndDate varchar(45);

IF LENGTH(p_StartDateTime)=19 THEN
set varStartDate=concat(substring(p_StartDateTime,7,4),'-',
                        substring(p_StartDateTime,4,2),'-',
                        substring(p_StartDateTime,1,2),' ',
                        substring(p_StartDateTime,12,8));
ELSE
set varStartDate=concat(substring(p_StartDateTime,7,4),'-',
                        substring(p_StartDateTime,4,2),'-',
                        substring(p_StartDateTime,1,2),' 00:00:00');

END IF;

IF LENGTH(p_EndDateTime)=19 THEN
set varEndDate=concat(substring(p_EndDateTime,7,4),'-',
                        substring(p_EndDateTime,4,2),'-',
                        substring(p_EndDateTime,1,2),' ',
                        substring(p_EndDateTime,12,8));
ELSE
set varEndDate=concat(substring(p_EndDateTime,7,4),'-',
                        substring(p_EndDateTime,4,2),'-',
                        substring(p_EndDateTime,1,2),' 23:59:59');

END IF;



select DATE_FORMAT(`starttime`,'%d/%m/%Y %T') as `Start Time`,DATE_FORMAT(`answertime`,'%d/%m/%Y %T') as `Answer Time`,DATE_FORMAT(`endtime`,'%d/%m/%Y %T') as `End Time`,
cd.Type,cr.partnerName as 'Originating partner',r.description as 'Originating Route',
cr1.partnerName as 'Terminating partner',r1.description as 'Terminating Route',
TerminatingCalledNumber as 'Called Number',TerminatingCallingNumber as 'Calling Number',durationsec as 'Duration in Seconds'
from cdr c
left join Enumservicegroup cd
on c.servicegroup=cd.id
left join Route r
on c.incomingroute=r.routename
and c.inpartnerid=r.idpartner
and c.switchid=r.switchid
left join partner cr
on r.idpartner=cr.idpartner
left join Route r1
on c.outgoingroute=r1.routename
and c.outpartnerid=r1.idpartner
and c.switchid=r1.switchid
left join partner cr1
on r1.idpartner=cr1.idpartner
where c.starttime>=varStartDate
and c.starttime<=varEndDate
and chargingstatus=1
order by starttime desc;

END //
DELIMITER ;


drop procedure if exists Country;
DELIMITER //
CREATE PROCEDURE Country()
BEGIN
select code,Country
from
(
select (select -1) as code,(select ' [All]') as Country,(select '    ') as name
union all
select Code,concat(name,' (',code,')') as Country,name from countrycode
) x
order by name,code ;
END //
DELIMITER ;


drop procedure if exists OutgoingPrefix;
DELIMITER //
CREATE PROCEDURE OutgoingPrefix(IN p_CountryCode varchar(45))
BEGIN

IF p_CountryCode<>'-1' THEN

select (select concat(':',p_CountryCode)) as Prefix,(select ' [All]') as Destination
union all
select Prefix,concat(Prefix,' (',description,')') as Destination
from xyzprefix
where CountryCode=p_CountryCode
order by Destination;

ELSE

select (select '-1') as Prefix,(select ' [All]') as Destination
union all
select id*(-1) as Prefix,concat(' [Pre-Selected]-',Name) as Prefix from xyzprefixset
union all
select Prefix,concat(Prefix,' (',description,')') as Destination
from xyzprefix
order by Destination;

END IF;

END //
DELIMITER ;



drop procedure if exists AllIntlpartnersRoute;
DELIMITER //
CREATE PROCEDURE AllIntlpartnersRoute()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idroute as idpartner,concat(RouteName,' (',cr.partnerName,')') as partnerName
from route r
left join partner cr
on r.idpartner=cr.idpartner
where cr.partnerType=3
order by partnername;

END //
DELIMITER ;

drop procedure if exists AllICXRoute;
DELIMITER //
CREATE PROCEDURE AllICXRoute()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idroute as idpartner,concat(RouteName,' (',cr.partnerName,')') as partnerName
from route r
left join partner cr
on r.idpartner=cr.idpartner
where cr.partnerType=2
order by partnername;


END //
DELIMITER ;




#Nibeer
drop procedure if exists CDRLGetRows;
DELIMITER //
CREATE  PROCEDURE `CDRLGetRows`(IN RowIndex  int, IN MaxRows int,IN QueryString varchar(1000) )
BEGIN

Declare StartRow Int;
Declare EndRow Int;

SET StartRow = (RowIndex+1);
SET EndRow = StartRow + MaxRows;


SET @query := CONCAT(QueryString, " LIMIT ",StartRow,",",EndRow);
  PREPARE stmt FROM @query;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;

END//
DELIMITER ;


drop procedure if exists CDRLGetCount;
delimiter $$

CREATE PROCEDURE `CDRLGetCount`(IN TableName varchar(50) )
BEGIN


#SELECT count(*) FROM TableName;
  SET @query := CONCAT("SELECT count(*) as UserCOUNT from ", TableName);
  PREPARE stmt FROM @query;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;

END $$
DELIMITER ;

drop procedure if exists CDRLGetRows;
delimiter $$


CREATE PROCEDURE `CDRLGetRows`(IN RowIndex  int, IN MaxRows int,IN QueryString varchar(1000) )
BEGIN

Declare StartRow Int;
Declare EndRow Int;

SET StartRow = (RowIndex+1);
SET EndRow = StartRow + MaxRows;


SET @query := CONCAT(QueryString, " LIMIT ",StartRow,",",EndRow);
  PREPARE stmt FROM @query;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;

END $$
DELIMITER ;

drop procedure if exists CDRLGetRows1;
delimiter $$
CREATE PROCEDURE `CDRLGetRows1`(IN RowIndex  int, IN MaxRows int,IN QueryString longtext )
BEGIN

Declare StartRow Int;
Declare EndRow Int;

SET StartRow = (RowIndex+0);
SET EndRow =  MaxRows+1;


SET @query := CONCAT(QueryString, " LIMIT ",StartRow,",",EndRow);
  PREPARE stmt FROM @query;
  EXECUTE stmt;
  DEALLOCATE PREPARE stmt;

END $$
DELIMITER ;

drop procedure if exists CDRLGetUsers;
delimiter $$

CREATE  PROCEDURE `CDRLGetUsers`(IN RowIndex  int, IN MaxRows int )
BEGIN

Declare StartRow Int;
Declare EndRow Int;

SET StartRow = (RowIndex+1);
SET EndRow = StartRow + MaxRows;

SELECT * FROM Telcobrightmediation.CDRListed
Order By FileName desc
Limit StartRow, EndRow;

END $$
DELIMITER ;

drop procedure if exists AllRoute;
DELIMITER //
CREATE PROCEDURE AllRoute()
BEGIN

select (select -1) as idpartner,(select ' All') as partnerName
union all
select idroute as idpartner,concat(RouteName,' (',cr.partnerName,')') as partnerName
from route r
left join partner cr
on r.idpartner=cr.idpartner
order by partnername;



END //
DELIMITER ;

DELIMITER $$
drop procedure if exists sp_exec_single_statement$$
CREATE PROCEDURE sp_exec_single_statement (IN command longtext,IN expectedRecCount int)
BEGIN
	declare affectedRecCount int;
    SET @query = command;
	PREPARE stmt FROM @query;
	EXECUTE stmt;
	set affectedRecCount= ROW_COUNT();
    if affectedRecCount=expectedRecCount then
		select affectedRecCount;
	else
		select -1;
    end if;
    deallocate prepare stmt;
END$$
DELIMITER ;

drop procedure if exists sp_exec_multiple_statement;
delimiter //
CREATE PROCEDURE sp_exec_multiple_statement(IN expectedRecCount int)
BEGIN
  DECLARE done INT DEFAULT FALSE;
  DECLARE statement_ text;
  DECLARE id_ INT;
  declare affectedRecCount int;
  DECLARE cur1 CURSOR FOR SELECT id,statement FROM temp_sql_statement;
  DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

  set affectedRecCount=0;
  OPEN cur1;
  read_loop: LOOP
    FETCH cur1 INTO id_, statement_;
	IF done THEN
      LEAVE read_loop;
    END IF;
    set @query=statement_;
	PREPARE stmt FROM @query;
	EXECUTE stmt;
    set affectedRecCount=affectedRecCount+ ROW_COUNT();
	deallocate PREPARE stmt;
  END LOOP;
  CLOSE cur1;
  if affectedRecCount=expectedRecCount then
		select affectedRecCount;
	else
		select -1;
end if;
END;//
delimiter ;




#Drop Trigger Part, execute after dump restore, when all tables exist

delimiter //
drop trigger transaction_after_insert//
CREATE TRIGGER transaction_after_insert
AFTER INSERT
   ON acc_transaction FOR EACH ROW

BEGIN

   update transactionmeta
   set totalInsertedAmount=totalInsertedAmount+NEW.amount;
END//

drop trigger transaction_delete//
CREATE TRIGGER transaction_delete
before DELETE
   ON acc_transaction FOR EACH ROW

BEGIN
	update transactionmeta
	set totalInsertedAmount=totalInsertedAmount-old.amount
    where id=1;
END//
delimiter ;

delimiter //
drop trigger acc_ledger_summary_after_insert//
CREATE TRIGGER acc_ledger_summary_after_insert
AFTER INSERT
   ON acc_ledger_summary FOR EACH ROW

BEGIN

   update ledger_summary_meta
   set totalInsertedAmount=totalInsertedAmount+NEW.amount;

END//

drop trigger acc_ledger_summary_after_update//
CREATE TRIGGER acc_ledger_summary_after_update
AFTER update
   ON acc_ledger_summary FOR EACH ROW

BEGIN

   update ledger_summary_meta
   set totalInsertedAmount=totalInsertedAmount+NEW.amount-OLD.amount;

END//



delimiter ;

delimiter //
drop TRIGGER cdr_after_insert//
CREATE TRIGGER cdr_after_insert
AFTER INSERT
   ON cdr FOR EACH ROW

BEGIN

   update cdrmeta
   set totalInsertedDuration=totalInsertedDuration+NEW.durationsec;
END//

drop TRIGGER cdr_delete//
CREATE TRIGGER cdr_delete
before DELETE
   ON cdr FOR EACH ROW

BEGIN
	update cdrmeta
	set totalInsertedDuration=totalInsertedDuration-old.durationsec
    where id=1;
END//
delimiter ;


delimiter //
drop TRIGGER sum_voice_day_01_after_insert//
CREATE TRIGGER sum_voice_day_01_after_insert
AFTER INSERT
   ON sum_voice_day_01 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_01
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_02_after_insert//
CREATE TRIGGER sum_voice_day_02_after_insert
AFTER INSERT
   ON sum_voice_day_02 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_02
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_03_after_insert//
CREATE TRIGGER sum_voice_day_03_after_insert
AFTER INSERT
   ON sum_voice_day_03 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_03
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_04_after_insert//
CREATE TRIGGER sum_voice_day_04_after_insert
AFTER INSERT
   ON sum_voice_day_04 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_04
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_05_after_insert//
CREATE TRIGGER sum_voice_day_05_after_insert
AFTER INSERT
   ON sum_voice_day_05 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_05
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_06_after_insert//
CREATE TRIGGER sum_voice_day_06_after_insert
AFTER INSERT
   ON sum_voice_day_06 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_06
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;



#update triggers

delimiter //
drop TRIGGER sum_voice_day_01_after_update//
CREATE TRIGGER sum_voice_day_01_after_update
AFTER update
   ON sum_voice_day_01 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_01
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_02_after_update//
CREATE TRIGGER sum_voice_day_02_after_update
AFTER update
   ON sum_voice_day_02 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_02
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_03_after_update//
CREATE TRIGGER sum_voice_day_03_after_update
AFTER update
   ON sum_voice_day_03 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_03
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_04_after_update//
CREATE TRIGGER sum_voice_day_04_after_update
AFTER update
   ON sum_voice_day_04 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_04
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_day_05_after_update//
CREATE TRIGGER sum_voice_day_05_after_update
AFTER update
   ON sum_voice_day_05 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_05
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;


delimiter //
drop TRIGGER sum_voice_day_06_after_update//
CREATE TRIGGER sum_voice_day_06_after_update
AFTER update
   ON sum_voice_day_06 FOR EACH ROW

BEGIN

   update cdrsummarymeta_day_06
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_01_after_insert//
CREATE TRIGGER sum_voice_hr_01_after_insert
AFTER INSERT
   ON sum_voice_hr_01 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_01
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_02_after_insert//
CREATE TRIGGER sum_voice_hr_02_after_insert
AFTER INSERT
   ON sum_voice_hr_02 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_02
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_03_after_insert//
CREATE TRIGGER sum_voice_hr_03_after_insert
AFTER INSERT
   ON sum_voice_hr_03 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_03
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_04_after_insert//
CREATE TRIGGER sum_voice_hr_04_after_insert
AFTER INSERT
   ON sum_voice_hr_04 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_04
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_05_after_insert//
CREATE TRIGGER sum_voice_hr_05_after_insert
AFTER INSERT
   ON sum_voice_hr_05 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_05
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_06_after_insert//
CREATE TRIGGER sum_voice_hr_06_after_insert
AFTER INSERT
   ON sum_voice_hr_06 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_06
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration;

END;//
delimiter ;


#update triggers

delimiter //
drop TRIGGER sum_voice_hr_01_after_update//
CREATE TRIGGER sum_voice_hr_01_after_update
AFTER update
   ON sum_voice_hr_01 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_01
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_02_after_update//
CREATE TRIGGER sum_voice_hr_02_after_update
AFTER update
   ON sum_voice_hr_02 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_02
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_03_after_update//
CREATE TRIGGER sum_voice_hr_03_after_update
AFTER update
   ON sum_voice_hr_03 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_03
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_04_after_update//
CREATE TRIGGER sum_voice_hr_04_after_update
AFTER update
   ON sum_voice_hr_04 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_04
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;

delimiter //
drop TRIGGER sum_voice_hr_05_after_update//
CREATE TRIGGER sum_voice_hr_05_after_update
AFTER update
   ON sum_voice_hr_05 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_05
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;


delimiter //
drop TRIGGER sum_voice_hr_06_after_update//
CREATE TRIGGER sum_voice_hr_06_after_update
AFTER update
   ON sum_voice_hr_06 FOR EACH ROW

BEGIN

   update cdrsummarymeta_hr_06
   set totalInsertedDuration=totalInsertedDuration+NEW.actualduration-OLD.actualduration;

END;//
delimiter ;
