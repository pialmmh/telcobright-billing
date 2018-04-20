package com.telcobright.App; /**
 * Created by Omnia on 8/2/2017.
 */

import com.telcobright.Main.SshCommandSender;
import com.telcobright.Session.*;
import org.springframework.context.support.ClassPathXmlApplicationContext;

import java.io.*;

public class IpLogExecuterMain {

    public static void main(String[] args) {
        try {
            //testHibernate();
            ClassPathXmlApplicationContext context = new ClassPathXmlApplicationContext("beans/IpLogBeans.xml");
            CliCommandSequence commandSequence = new CliCommandSequence();
            SshSessionInfo sessionInfo = context.getBean("ne1SessionInfo", SshSessionInfo.class);

            SshCommandSender commandSender = new SshCommandSender();
            commandSender.setCliCommandSequence(commandSequence);
            commandSender.setSessionInfo(sessionInfo);


            String output = commandSender.sendCommand();
            System.out.println("The output received is");
            System.out.println(output);
            /*ICliOutputParser parser = new CiscoShMacAddTableResultParser();
            List<CiscoShMacAddTableResult> parsedObjects = parser.parse(output);
            int affectedRecords = saveToDb(parsedObjects, context);
            System.out.println(affectedRecords + " logs inserted.");*/
        } catch (Exception e) {
            e.printStackTrace();
        }
    }



    /*static void testHibernate(){
        Configuration cfg=new Configuration();
        cfg.configure("hibernate.cfg.xml");//populates the data of the configuration file

        //creating seession factory object
        SessionFactory factory=cfg.buildSessionFactory();

        //creating session object
        Session session=factory.openSession();

        //creating transaction object
        Transaction t=session.beginTransaction();

        EmployeeEn e1=new EmployeeEn();
        e1.setId(115);
        e1.setFirstName("sonoo");
        e1.setLastName("jaiswal");

        session.persist(e1);//persisting the object

        t.commit();//transaction is commited
        session.close();

        System.out.println("successfully saved");
    }

    static int saveToDb(List<CiscoShMacAddTableResult> instances,ClassPathXmlApplicationContext context ) {
        ISpringQueryExecuter springQueryExecuter =
                context.getBean("springQueryExecuter", MySqlJdblSpringQueryExecuter.class);
        ISqlBuilder sqlBuilder = SqlBuilderFactory.getSqlBuilder("CiscoShMacAddTableResult");
        CiscoShMacAddTableResultParserCRUD crudExecuter = new CiscoShMacAddTableResultParserCRUD(springQueryExecuter, sqlBuilder);
        return crudExecuter.insertMany(instances);
    }*/


    static ByteArrayOutputStream setConsoleToBuffer(PrintStream shellStream) {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        System.setOut(shellStream);//console to buffer
        return baos;
    }

    /*static String resetConsoleAndGetOutput(ByteArrayOutputStream baos,long timeOut) throws Exception {

        shellStream.flush();
        long sleepInterval = 200;
        long elapsedTime = 0;
        while (baos.size() < 1) {
            Thread.sleep(sleepInterval);
            elapsedTime += sleepInterval;
            shellStream.flush();
            if (elapsedTime >= timeOut)
                break;
        }
        System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out)));
        return baos.toString();
    }*/
    static boolean checkPromptPosition(String prompt) {

        return false;
    }

    public static void setConsoleToFile(String outputFileName) throws FileNotFoundException {
        File file = new File(outputFileName); //Your file
        FileOutputStream fos = new FileOutputStream(file);
        PrintStream ps = new PrintStream(fos);
        System.setOut(ps);

        //System.out.println("This goes to out.txt");
    }


    static void WriteLineToLog(String fileName, String line) throws Exception {
        try {
            FileWriter fstream = new FileWriter(fileName, true);
            BufferedWriter out = new BufferedWriter(fstream);
            out.write(line);
            out.newLine();
            //close buffer writer
            out.close();
        } catch (IOException e) {
            e.printStackTrace();
            throw e;

        }

    }
}
