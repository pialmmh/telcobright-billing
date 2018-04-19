package dao.CRUD;

import dao.Common.ISpringQueryExecuter;
import dao.Common.ISqlBuilder;
import dao.Models.CiscoShMacAddTableResult;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by Mustafa on 8/30/2017.
 */
public class CiscoShMacAddTableResultParserCRUD {
    ISpringQueryExecuter queryExecuter;
    ISqlBuilder sqlBuilder;
    public CiscoShMacAddTableResultParserCRUD(ISpringQueryExecuter queryExecuter, ISqlBuilder sqlBuilder) {
        this.queryExecuter=queryExecuter;
        this.sqlBuilder=sqlBuilder;
    }
    public int insertMany(List<CiscoShMacAddTableResult> instances){
        List<String> insertStatementsValueParts=new ArrayList<String>();
        for (CiscoShMacAddTableResult instance:instances) {
            insertStatementsValueParts.add(sqlBuilder.getExtInsertValues(instance));
        }
        int affectedRecords=queryExecuter.create(sqlBuilder.getExtInsertHeader()+"\n"
                +String.join(",",insertStatementsValueParts)+";");
        return affectedRecords;
    }
}
