package dao.Models;

import javax.persistence.*;

/**
 * Created by Mustafa on 9/6/2017.
 */
@Entity
@Table(name = "autoincrementcounter", schema = "iplog", catalog = "")
public class AutoincrementcounterEn {
    private String tableName;
    private long value;

    @Id
    @Column(name = "tableName")
    public String getTableName() {
        return tableName;
    }

    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    @Basic
    @Column(name = "value")
    public long getValue() {
        return value;
    }

    public void setValue(long value) {
        this.value = value;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        AutoincrementcounterEn that = (AutoincrementcounterEn) o;

        if (value != that.value) return false;
        if (tableName != null ? !tableName.equals(that.tableName) : that.tableName != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = tableName != null ? tableName.hashCode() : 0;
        result = 31 * result + (int) (value ^ (value >>> 32));
        return result;
    }
}
