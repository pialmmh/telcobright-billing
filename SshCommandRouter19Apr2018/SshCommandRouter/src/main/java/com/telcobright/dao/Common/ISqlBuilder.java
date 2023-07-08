package com.telcobright.dao.Common;

/**
 * Created by Mustafa on 8/30/2017.
 */
public interface ISqlBuilder<T> {
    public  String getExtInsertHeader();
    public String getExtInsertValues(T instance);
}
