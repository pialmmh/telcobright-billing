package com.telcobright.dao.Models;

import java.util.Date;

/**
 * Created by Omnia on 8/9/2017.
 */
public class CiscoShMacAddTableResult {
    private int vlan;
    private String macAddress;
    private String type;
    private String learn;
    private int age;    // age in seconds
    private String port;
    private boolean primary;
    private Date logTime;

    public CiscoShMacAddTableResult() {

    }

    public CiscoShMacAddTableResult(int vlan, String address, String type, String learn, int age, String port,
                                    boolean primary,Date logTime) {
        this.vlan = vlan;
        this.macAddress = address;
        this.type = type;
        this.learn = learn;
        this.age = age;
        this.port = port;
        this.primary=primary;
        this.logTime=logTime;
    }


    public String getMacAddress() {
        return macAddress;
    }

    public void setMacAddress(String macAddress) {
        this.macAddress = macAddress;
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    public int getVlan() {
        return vlan;
    }

    public void setVlan(int vlan) {
        this.vlan = vlan;
    }

    public String getLearn() {
        return learn;
    }

    public void setLearn(String learn) {
        this.learn = learn;
    }

    public int getAge() {
        return age;
    }
    public  void  setAge(int age){this.age=age;}

    public String getPort() {
        return port;
    }

    public  void setPort(String port){this.port=port;}


    public String toString(){

        return primary + "\t" + vlan + "\t" + macAddress + "\t" + type + "\t"  + learn + "\t" + age + "\t" + port;
    }
    public boolean getPrimary() {
        return primary;
    }

    public void setPrimary(boolean primary) {
        this.primary = primary;
    }

    public Date getLogTime() {
        return logTime;
    }

    public void setLogTime(Date logTime) {
        this.logTime = logTime;
    }
}

