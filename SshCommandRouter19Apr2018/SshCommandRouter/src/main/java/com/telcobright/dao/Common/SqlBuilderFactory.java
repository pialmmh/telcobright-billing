package com.telcobright.dao.Common;

import com.telcobright.dao.SqlBuilder.CiscoShMacAddTableResultSqlBuilder;

/**
 * Created by Mustafa on 8/30/2017.
 */
public class SqlBuilderFactory{
    public static ISqlBuilder getSqlBuilder(String entityName){
        switch (entityName){
            case "CiscoShMacAddTableResult":
                return new CiscoShMacAddTableResultSqlBuilder();
                default:
                    break;
        }
        throw new UnsupportedOperationException();
    }
}
