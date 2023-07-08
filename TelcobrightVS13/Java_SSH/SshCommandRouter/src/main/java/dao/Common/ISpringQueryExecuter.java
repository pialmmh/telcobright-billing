package dao.Common;

import javax.sql.DataSource;
import java.util.List;

/**
 * Created by Administrator on 8/23/2017.
 */
public interface ISpringQueryExecuter<T> {

    public void setDataSource(DataSource ds);

    public int create(String query);

    public List<T> retrieve(String query);

    public int update(String query);

    public void execute(String query);

    public int delete(String query);
}
