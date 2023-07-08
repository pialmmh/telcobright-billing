package com.telcobright.dao.Common;

/**
 * Created by Administrator on 8/30/2017.
 */
public class SpringJdbcSelectorFactory {
    public static ISpringQueryExecuter getSpringQueryExecuter(String DbEnginName) {
        switch (DbEnginName) {
            case "mysql":
                return new MySqlJdblSpringQueryExecuter();
            default:
                break;
        }
        throw new UnsupportedOperationException();
    }
}
