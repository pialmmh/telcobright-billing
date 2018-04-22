package dao.Common;

import dao.Common.ISpringQueryExecuter;
import org.springframework.dao.DataAccessException;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.jdbc.core.ResultSetExtractor;

import javax.sql.DataSource;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Created by Administrator on 8/22/2017.
 */
public class MySqlJdblSpringQueryExecuter implements ISpringQueryExecuter {
    private JdbcTemplate jdbcTemplate;

    public void setJdbcTemplate(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public void setDataSource(DataSource ds) {

    }

    @Override
    public int create(String query) {
        if (query!=null && !query.isEmpty()){
            return jdbcTemplate.update(query);
        }
        return 0;
    }

    @Override
    public List retrieve(String query) {
        List<Object> Data = new ArrayList<>();
        if (query!=null && !query.isEmpty()){
            return  jdbcTemplate.query(query, new ResultSetExtractor<List>() {
                @Override
                public List extractData(ResultSet rs) throws SQLException, DataAccessException {
                    ResultSetMetaData rsmd = rs.getMetaData();
                    while (rs.next()){
                        Map<Object,Object> rData=new HashMap<>();
                        for (int i = 1; i <= rsmd.getColumnCount(); i++) {
                            rData.put(rsmd.getColumnName(i),rs.getString(i));;
                        }
                        Data.add(rData);

                    }
                    return Data;
                }
            });
        }
        return null;
    }

    @Override
    public int update(String query) {
        if (query!=null && !query.isEmpty()){
            return jdbcTemplate.update(query);
        }
        return 0;
    }

    @Override
    public void execute(String query) {
           jdbcTemplate.execute(query);
    }

    @Override
    public int delete(String query) {
        if (query!=null && !query.isEmpty()){
            return jdbcTemplate.update(query);
        }
        return 0;
    }

}
