package com.telcobright.dao.Models;

import javax.persistence.*;
import java.sql.Timestamp;

/**
 * Created by Mustafa on 9/6/2017.
 */
@Entity
@Table(name = "ciscoshmacaddtableresult", schema = "iplog", catalog = "")
public class CiscoshmacaddtableresultEn {
    @Id
    private long id;
    private Integer vlan;
    private String macAddress;
    private String type;
    private String learn;
    private Integer age;
    private String port;
    private Byte primary;
    private Timestamp logTime;

    @Basic
    @Column(name = "id")
    public long getId() {
        return id;
    }

    public void setId(long id) {
        this.id = id;
    }

    @Basic
    @Column(name = "vlan")
    public Integer getVlan() {
        return vlan;
    }

    public void setVlan(Integer vlan) {
        this.vlan = vlan;
    }

    @Basic
    @Column(name = "macAddress")
    public String getMacAddress() {
        return macAddress;
    }

    public void setMacAddress(String macAddress) {
        this.macAddress = macAddress;
    }

    @Basic
    @Column(name = "type")
    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    @Basic
    @Column(name = "learn")
    public String getLearn() {
        return learn;
    }

    public void setLearn(String learn) {
        this.learn = learn;
    }

    @Basic
    @Column(name = "age")
    public Integer getAge() {
        return age;
    }

    public void setAge(Integer age) {
        this.age = age;
    }

    @Basic
    @Column(name = "port")
    public String getPort() {
        return port;
    }

    public void setPort(String port) {
        this.port = port;
    }

    @Basic
    @Column(name = "primary")
    public Byte getPrimary() {
        return primary;
    }

    public void setPrimary(Byte primary) {
        this.primary = primary;
    }

    @Basic
    @Column(name = "logTime")
    public Timestamp getLogTime() {
        return logTime;
    }

    public void setLogTime(Timestamp logTime) {
        this.logTime = logTime;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        CiscoshmacaddtableresultEn that = (CiscoshmacaddtableresultEn) o;

        if (id != that.id) return false;
        if (vlan != null ? !vlan.equals(that.vlan) : that.vlan != null) return false;
        if (macAddress != null ? !macAddress.equals(that.macAddress) : that.macAddress != null) return false;
        if (type != null ? !type.equals(that.type) : that.type != null) return false;
        if (learn != null ? !learn.equals(that.learn) : that.learn != null) return false;
        if (age != null ? !age.equals(that.age) : that.age != null) return false;
        if (port != null ? !port.equals(that.port) : that.port != null) return false;
        if (primary != null ? !primary.equals(that.primary) : that.primary != null) return false;
        if (logTime != null ? !logTime.equals(that.logTime) : that.logTime != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = (int) (id ^ (id >>> 32));
        result = 31 * result + (vlan != null ? vlan.hashCode() : 0);
        result = 31 * result + (macAddress != null ? macAddress.hashCode() : 0);
        result = 31 * result + (type != null ? type.hashCode() : 0);
        result = 31 * result + (learn != null ? learn.hashCode() : 0);
        result = 31 * result + (age != null ? age.hashCode() : 0);
        result = 31 * result + (port != null ? port.hashCode() : 0);
        result = 31 * result + (primary != null ? primary.hashCode() : 0);
        result = 31 * result + (logTime != null ? logTime.hashCode() : 0);
        return result;
    }
}
