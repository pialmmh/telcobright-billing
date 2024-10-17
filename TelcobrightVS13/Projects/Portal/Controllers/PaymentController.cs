using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using MediationModel;
using MySql.Data.MySqlClient;
using PortalApp._portalHelper;
using TelcobrightMediation;

namespace WebApiController.Controllers
{
    [RoutePrefix("api/payment")]
    public class PaymentController : ApiController
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
        private readonly TelcobrightConfig _tbc = PageUtil.GetTelcobrightConfig();
        private readonly PartnerEntities _context = new PartnerEntities();

        [HttpGet]
        [Route("getAllAccount")]
        public IHttpActionResult GetAllAccounts([FromBody] Dictionary<string,string> payload)
        {
            try
            {
                string starttime;
                payload.TryGetValue("starttime", out starttime);
                string endtime;
                payload.TryGetValue("endtime", out endtime);
                string partnerId;
                payload.TryGetValue("idpartner", out partnerId);
                int idpartner = Convert.ToInt32(partnerId);
                Func<string> getWhereClauseForIdPartner = () => idpartner > -1 ? $"where ac.idpartner ={idpartner}" : "";
                List<AccountBalanceInfo> accountList = new List<AccountBalanceInfo>();

                string query = $@"SELECT 
                               DATE(ac_t.transactionTime) AS balanceDate,
                               p.PartnerName AS applicationName,
                               SUM(ac_t.balanceBefore) AS initialBalance,
                               SUM(ac_t.amount) AS paymentAmount,
                               SUM(ac_t.balanceAfter) AS currentBalance
                           FROM
                               (
                               SELECT * FROM acc_transaction 
                                WHERE transactionTime >= '{starttime}'
                                AND transactionTime < '{endtime}'
                                ) AS ac_t
                               LEFT JOIN account ac ON ac_t.glAccountId = ac.id
                               LEFT JOIN partner p ON ac.idPartner = p.idPartner
                               {getWhereClauseForIdPartner}
                           GROUP BY 
                               balanceDate, p.idPartner";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        connection.Open();

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AccountBalanceInfo account = new AccountBalanceInfo
                                {
                                    BalanceDate = reader.GetDateTime(reader.GetOrdinal("balanceDate")),
                                    ApplicationName = reader.GetString(reader.GetOrdinal("applicationName")),
                                    InitialBalance = reader.GetDecimal(reader.GetOrdinal("initialBalance")),
                                    PaymentAmount = reader.IsDBNull(reader.GetOrdinal("paymentAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("paymentAmount")),
                                    CurrentBalance = reader.GetDecimal(reader.GetOrdinal("currentBalance"))
                                };
                                accountList.Add(account);
                            }
                        }
                    }
                }

                return Ok(accountList);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("addPayment")]
        [HttpPost]
        public IHttpActionResult AddPayment(PaymentHistoryInfo paymentHistory)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            PaymentHistoryInfo tempinfo = _context.accounts
                                .Where(rows => rows.idPartner == paymentHistory.PartnerId && rows.serviceGroup == 4)
                                .Select(rows => new PaymentHistoryInfo
                                {
                                    AccountId = rows.id,
                                    BalanceBefore = rows.balanceBefore,
                                    BalanceAfter = rows.balanceAfter,
                                    PaymentAmount = rows.lastAmount 
                                })
                                .FirstOrDefault();

                            decimal? balanceBefore = tempinfo.BalanceAfter;
                            decimal? balanceAfter = tempinfo.BalanceAfter - tempinfo.PaymentAmount;
                            decimal? paymentAmount = tempinfo.PaymentAmount;
                            // First update the account balances for the specific partner and service group
                            string updateQuery = @"UPDATE account 
                                           SET balanceAfter =@BalanceAfter, balanceBefore = @BalanceBefore, lastAmount = @PaymentAmount, lastUpdated = @Date 
                                           WHERE id = @Id";

                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@Id", tempinfo.AccountId);
                                updateCommand.Parameters.AddWithValue("@Date", DateTime.Now); 
                                updateCommand.Parameters.AddWithValue("@PaymentAmount", paymentAmount);
                                updateCommand.Parameters.AddWithValue("@Type", paymentHistory.Type);
                                updateCommand.Parameters.AddWithValue("@BalanceBefore", balanceBefore);
                                updateCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                                updateCommand.ExecuteNonQuery();
                            }

                            // Then insert the payment details into the payment_history table
                            string insertQuery = @"SET FOREIGN_KEY_CHECKS=0;
                                           INSERT INTO payment_history (AccountId, PartnerId, PartnerName, Service, paymentDate, PaymentAmount, paymentType, BalanceBefore, BalanceAfter, TransactionDetails, Reference)
                                           VALUES (@AccountId, @PartnerId, @PartnerName, @Service, @Date, @PaymentAmount, @Type, @BalanceBefore, @BalanceAfter, @TransactionDetails, @Reference)";

                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection, transaction))
                            {
                                insertCommand.Parameters.AddWithValue("@AccountId", paymentHistory.AccountId);
                                insertCommand.Parameters.AddWithValue("@PartnerId", paymentHistory.PartnerId);
                                insertCommand.Parameters.AddWithValue("@PartnerName", paymentHistory.PartnerName);
                                insertCommand.Parameters.AddWithValue("@Service", paymentHistory.Service);
                                insertCommand.Parameters.AddWithValue("@Date", DateTime.Now); 
                                insertCommand.Parameters.AddWithValue("@PaymentAmount", paymentHistory.PaymentAmount);
                                insertCommand.Parameters.AddWithValue("@Type", paymentHistory.Type);
                                insertCommand.Parameters.AddWithValue("@BalanceBefore", balanceBefore);
                                insertCommand.Parameters.AddWithValue("@BalanceAfter", balanceAfter);
                                insertCommand.Parameters.AddWithValue("@TransactionDetails", paymentHistory.TransactionDetails);
                                insertCommand.Parameters.AddWithValue("@Reference", paymentHistory.Reference);

                                insertCommand.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return BadRequest("Error: " + ex.Message);
                        }
                    }
                }

                return Ok("Payment details added successfully, and account balances updated.");
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }
        // query = @"SET FOREIGN_KEY_CHECKS=0;
    }
    public class AccountBalanceInfo
    {
        public DateTime BalanceDate { get; set; }
        public string ApplicationName { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal PaymentAmount { get; set; }
        //public decimal TotalMin { get; set; }
        //public decimal TotalCost { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    // Model class for Payment
    public class Payment
    {
        public long Id { get; set; }
        public int IdPartner { get; set; }
        public string AccountName { get; set; }
        public int ServiceGroup { get; set; }
        public int ServiceFamily { get; set; }
        public int Product { get; set; }
        public string BillableType { get; set; }
        public string UOM { get; set; }
        public int Depth { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal? LastAmount { get; set; }
        public decimal BalanceAfter { get; set; }
        public int? SuperviseNegativeBalance { get; set; }
        public decimal NegativeBalanceLimit { get; set; }
    }
}
