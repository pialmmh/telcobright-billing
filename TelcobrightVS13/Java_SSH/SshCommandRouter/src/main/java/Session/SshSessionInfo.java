package Session;

import java.util.Properties;
import com.jcraft.jsch.Channel;
import com.jcraft.jsch.ChannelShell;
import com.jcraft.jsch.JSch;
import com.jcraft.jsch.Session;

/**
 * Created by Mustafa on 8/8/2017.
 */
public class SshSessionInfo implements ISessionInfo {
    private Properties config;
    private String userName;
    private String password;
    private String hostName;
    private int networkPort;
    private CliSessionType cliSessionType;
    private Session session;
    private Channel shellChannel;

    public void setCurrentPrompt(String currentPrompt) {
        this.currentPrompt = currentPrompt;
    }

    private String currentPrompt;

    public Session getSession() {
        return session;
    }
    public Channel getChannel(){
        return this.shellChannel;
    }
    public String getCurrentPrompt() {
        return currentPrompt;
    }
    public boolean checkExpectedPrompt(String lastOutput, String prompt)
    {
        if(lastOutput.endsWith(prompt)){
            currentPrompt=prompt;
            return true;
        }
        return false;
    }
    public void connect() throws Exception{
        this.session= (Session) SessionFactory.getSession(this);
        session.connect();
        shellChannel = session.openChannel("shell");
        shellChannel.connect();
    }

    public void setConfig(Properties config) {
        this.config=config;
    }

    public void setUserName(String userName) {
        this.userName=userName;
    }

    public void setPassword(String password) {
        this.password=password;
    }

    public void setHostName(String hostName) {
        this.hostName=hostName;
    }

    public void setNetworkPort(int networkPort) {
        this.networkPort=networkPort;
    }

    public void setCliSessionType(CliSessionType cliSessionType) {
        this.cliSessionType=cliSessionType;
    }

    public Properties getConfig() {
        return config;
    }

    public String getUserName() {
        return userName;
    }

    public String getPassword() {
        return password;
    }

    public String getHostName() {
        return hostName;
    }

    public int getNetworkPort() {
        return networkPort;
    }

    public CliSessionType getCliSessionType() {
        return cliSessionType;
    }


}
