package CliParser;

import dao.Models.CiscoShMacAddTableResult;
import org.apache.commons.validator.routines.IntegerValidator;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.List;

import java.util.ArrayList;


public class CiscoShMacAddTableResultParser implements ICliOutputParser {

    IntegerValidator integerValidator = new IntegerValidator();

    public List<CiscoShMacAddTableResult> parse(String cliOutput) {
        List<CiscoShMacAddTableResult> parsedObjects = new ArrayList<>();
        String[] lines=cliOutput.split("\\r?\\n");
        for (String line:lines) {
            line=line.trim();
            if(lineQualifies(line)==false) continue;
            CiscoShMacAddTableResult ciscoShMacAddTableResult=new CiscoShMacAddTableResult();
            boolean isPrimary=line.startsWith("*");
            String[] fields;
            if(isPrimary){
                fields = line.substring(1).trim().split(" +");
            }
            else{
                fields = line.trim().split(" +");
            }
            ciscoShMacAddTableResult.setPrimary(isPrimary);

        if (fields.length > 4)
        {
            IntegerValidator integerValidator=new IntegerValidator();
            Integer vlan= integerValidator.validate(fields[0]);
            Integer age= integerValidator.validate(fields[4]);
            if(vlan!=null){ciscoShMacAddTableResult.setVlan(vlan.intValue());}
                ciscoShMacAddTableResult.setMacAddress(fields[1]);
                ciscoShMacAddTableResult.setType(fields[2]);
                ciscoShMacAddTableResult.setLearn(fields[3]);
            if(age!=null){ciscoShMacAddTableResult.setAge(age.intValue());}
                ciscoShMacAddTableResult.setPort(fields[5]);
            Date timestamp = Calendar.getInstance().getTime();
            ciscoShMacAddTableResult.setLogTime(timestamp);
            parsedObjects.add(ciscoShMacAddTableResult);
            }
        }
        return parsedObjects;
    }
    boolean lineQualifies(String line) {
        return (line.contains("dynamic") || line.contains("static"))
                && (line.contains("Yes") || line.contains("No"));
    }

}
