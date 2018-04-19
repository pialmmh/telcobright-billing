package Session;
import com.jcraft.jsch.Channel;
import com.jcraft.jsch.Session;

import java.util.Properties;

/**
 * Created by Mustafa on 8/8/2017.
 */
public interface ISessionInfo {
    public void setConfig(Properties properties);
    public void setUserName(String userName);
    public void setPassword(String password);
    public void setHostName(String hostName);
    public void setNetworkPort(int networkPort);
    public void setCliSessionType(CliSessionType cliSessionType);
    public void setCurrentPrompt(String currentPrompt);
    public void setMoreOutputIndicator(String moreOutputIndicator);
    public void setStrToSendForMoreOutput(String strToSendForMoreOutput);
    public Properties getConfig();
    public String getUserName();
    public String getPassword();
    public String getHostName();
    public int getNetworkPort();
    public CliSessionType getCliSessionType();
    public void connect() throws Exception;
    public Session getSession();
    public String getCurrentPrompt();
    public String getMoreOutputIndicator();
    public String getStrToSendForMoreOutput();
    public Channel getChannel();
    public boolean checkExpectedPrompt(String lastOutput, String prompt);
}
