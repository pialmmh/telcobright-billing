package dao.SqlBuilder;

import dao.Common.ISqlBuilder;
import dao.Models.CiscoShMacAddTableResult;

/**
 * Created by Mustafa on 8/30/2017.
 */
public class CiscoShMacAddTableResultSqlBuilder implements ISqlBuilder {
    public String getExtInsertHeader() {

        return "insert into CiscoShMacAddTableResult (" +
                "vlan," +
                "macAddress," +
                "type," +
                "learn," +
                "age," +
                "port," +
                "`primary`," +
                "logTime ) values ";
    }

    public String getExtInsertValues(Object objInstance){
        CiscoShMacAddTableResult instance=(CiscoShMacAddTableResult)objInstance;
        java.text.SimpleDateFormat sdf =
                new java.text.SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        String strLogTime = sdf.format(instance.getLogTime());
        return "("+instance.getVlan()+"," +
                "'"+instance.getMacAddress()+"'," +
                "'"+instance.getType()+"'," +
                "'"+instance.getLearn()+"'," +
                +instance.getAge()+"," +
                "'"+instance.getPort()+"'," +
                +(instance.getPrimary()==true?1:0)+"," +
                "'"+strLogTime+"')";
    }
}
