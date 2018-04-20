package com.telcobright.Main;
import org.springframework.jdbc.core.JdbcTemplate;

/**
 * Created by Administrator on 8/21/2017.
 */
public class User {
    private String username;
    private String password;
    private int enable;

    public User() {

    }
    public User(String username, String password, int enable) {
        this.username = username;
        this.password = password;
        this.enable = enable;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public int getEnable() {
        return enable;
    }

    public void setEnable(int enable) {
        this.enable = enable;
    }

    @Override
    public String toString() {
        return "User{" +
                "username='" + username + '\'' +
                ", password='" + password + '\'' +
                ", enable=" + enable +
                '}';
    }
}
