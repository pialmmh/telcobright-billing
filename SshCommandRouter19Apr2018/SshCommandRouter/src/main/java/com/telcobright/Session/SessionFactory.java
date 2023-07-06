package com.telcobright.Session;
import com.jcraft.jsch.JSch;
import com.jcraft.jsch.Session;

/**
 * Created by Mustafa on 8/8/2017.
 */

public class SessionFactory {
    public static Runnable getSession(ISessionInfo cliSessioninfo) throws Exception{
        switch(cliSessioninfo.getCliSessionType())
        {
            case Ssh:
                JSch jsch = new JSch();
                Session session = jsch.getSession(cliSessioninfo.getUserName(),
                        cliSessioninfo.getHostName(), cliSessioninfo.getNetworkPort());
                session.setPassword(cliSessioninfo.getPassword());
                session.setConfig(cliSessioninfo.getConfig());
                return session;

            case Telnet:
                break;
        }
        return null;
    }

}
