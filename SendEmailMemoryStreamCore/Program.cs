using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SendEmailMemoryStreamCore
{
  class Program
  {
    static void Main(string[] args)
    {
      #region SQL

      // Connection String
      SqlConnection sqlConnection = new SqlConnection()
      {
        ConnectionString = "",
      };

      // Sql Command
      SqlCommand sqlCommand = new SqlCommand()
      {
        Connection = sqlConnection
      };

      try
      {
        sqlConnection.Open();

        string inputstring = "";
        var toByteArray = Encoding.UTF8.GetBytes(inputstring);

        // Insert BinaryData into VarBinary Column in Database
        sqlCommand.CommandText = "INSERT INTO VarBinaryTable VALUES (@input)";
        sqlCommand.Parameters.AddWithValue("@input", toByteArray);
        sqlCommand.ExecuteNonQuery();

        // Select * from VarBinaryTable
        sqlCommand.CommandText = "SELECT TOP 1 * FROM VarBinaryTable ORDER BY [Id] DESC";
        sqlCommand.ExecuteNonQuery();
      }
      catch (SqlException ex)
      {
        Console.WriteLine(ex.ToString());
        throw;
      }

      // Sql Data Reader
      SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

      #endregion SQL

      #region Stream-Reader-Writer

      // Create a Memory Stream and Set Position to 0
      MemoryStream stream = new MemoryStream()
      {
        Position = 0
      };

      // Create a Stream Writer
      StreamWriter streamWriter = new StreamWriter(stream);

      while (sqlDataReader.Read())
      {
        byte[] contentAsBytes = sqlDataReader.GetFieldValue<byte[]>(sqlDataReader.GetOrdinal("VarBinaryColumn"));
        // Write to file
        streamWriter.WriteLine(Encoding.UTF8.GetString(contentAsBytes));
      }

      // Flush the streamWriter
      streamWriter.Flush();
      // Set Stream Seek
      stream.Seek(0, SeekOrigin.Begin);

      sqlConnection.Close();

      #endregion Stream-Reader-Writer

      #region Email

      // Credentials
      NetworkCredential AuthenticationInfo = new NetworkCredential("username", "password");

      // SMTP Client
      SmtpClient smtpClient = new SmtpClient
      {
        Host = "IP",
        Port = 25,
        Credentials = AuthenticationInfo,
        EnableSsl = false,
      };

      // MailAddress From
      MailAddress from = new MailAddress("fromEmail@Address.com");

      // MailAddress To
      MailAddress to = new MailAddress("toEmail@Address.com");

      // Add mail addresses on the message
      MailMessage message = new MailMessage(from, to)
      {
        // Subject
        Subject = "This is a test mail!",

        // Message Body
        Body = "<p>Test Email</p>",

        // Encodings
        SubjectEncoding = Encoding.UTF8,
        BodyEncoding = Encoding.UTF8,
        IsBodyHtml = true
      };

      // Add the attachment to the message.
      message.Attachments.Add(new Attachment(stream, "testfile.csv", "text/csv"));

      Console.WriteLine($"Sending an email message to: {to}");
      Console.WriteLine($"Using SMTP host: {smtpClient.Host}");
      Console.WriteLine($"Port: {smtpClient.Port}");

      try
      {
        // Send Message
        smtpClient.Send(message);
        Console.WriteLine("Message sent successfully!!!!");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadLine();
      }
      catch (SmtpException smtpException)
      {
        Console.WriteLine(smtpException);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      #endregion Email
    }
  }
}
