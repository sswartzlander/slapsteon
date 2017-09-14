using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace AirQualityMonitoring
{
    public class DatabaseUtilities
    {
        public static void ExecuteStoredProcedure(MySqlConnection connection, string procName, params object[] parameters)
        {
            using (MySqlCommand command = new MySqlCommand(procName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                if (null != parameters)
                {
                    int i = 1;
                    foreach (object parameter in parameters)
                    {
                        string parameterName = "param" + i.ToString();
                        i++;

                        if (null == parameter)
                            command.Parameters.Add(CreateNullParameter(parameterName));
                        else if (typeof(Guid) == parameter.GetType())
                            command.Parameters.Add(CreateGuidParameter((Guid)parameter, parameterName));
                        else if (typeof(decimal) == parameter.GetType())
                            command.Parameters.Add(CreateDecimalParameter((decimal)parameter, parameterName));
                        else if (typeof(DateTime) == parameter.GetType())
                            command.Parameters.Add(CreateDateTimeParameter((DateTime)parameter, parameterName));
                        else if (typeof(string) == parameter.GetType())
                            command.Parameters.Add(CreateStringParameter((string)parameter, parameterName));
                        else if (typeof(int) == parameter.GetType())
                        {
                            command.Parameters.Add(CreateInt32Parameter((int)parameter, parameterName));
                            //command.Parameters.AddWithValue(parameterName, (int)parameter);
                        }

                        else if (typeof(bool) == parameter.GetType())
                            command.Parameters.Add(CreateBooleanParameter((bool)parameter, parameterName));
                        else
                            throw new Exception("Could not create database parameter for type: " + parameter.GetType().ToString());

                    }
                }

                command.ExecuteNonQuery();
            }
        }

        public static void ExecuteNonQuery(MySqlConnection connection, string sql, string errorMessage, params object[] parameters)
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;

                if (null != parameters)
                {
                    int i = 1;
                    foreach (object parameter in parameters)
                    {
                        string parameterName = "param" + i.ToString();
                        i++;

                        if (null == parameter)
                            command.Parameters.Add(CreateNullParameter(parameterName));
                        else if (typeof(Guid) == parameter.GetType())
                            command.Parameters.Add(CreateGuidParameter((Guid)parameter, parameterName));
                        else if (typeof(decimal) == parameter.GetType())
                            command.Parameters.Add(CreateDecimalParameter((decimal)parameter, parameterName));
                        else if (typeof(DateTime) == parameter.GetType())
                            command.Parameters.Add(CreateDateTimeParameter((DateTime)parameter, parameterName));
                        else if (typeof(string) == parameter.GetType())
                            command.Parameters.Add(CreateStringParameter((string)parameter, parameterName));
                        else if (typeof(int) == parameter.GetType())
                        {
                            command.Parameters.Add(CreateInt32Parameter((int)parameter, parameterName));
                            //command.Parameters.AddWithValue(parameterName, (int)parameter);
                        }

                        else if (typeof(bool) == parameter.GetType())
                            command.Parameters.Add(CreateBooleanParameter((bool)parameter, parameterName));
                        else
                            throw new Exception("Could not create database parameter for type: " + parameter.GetType().ToString());

                    }
                }

                command.ExecuteNonQuery();
            }

        }


        public static MySqlParameter CreateGuidParameter(Guid? value, string parameterName)
        {
            byte[] byteArray = null;
            if (null != value)
                byteArray = value.Value.ToByteArray();

            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.Binary);
            parameter.Value = EnsureDBNull(byteArray);

            return parameter;
        }

        public static MySqlParameter CreateDateTimeParameter(DateTime? value, string parameterName)
        {

            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.DateTime);
            parameter.Value = EnsureDBNull(value);

            return parameter;
        }

        public static MySqlParameter CreateDecimalParameter(decimal? value, string parameterName)
        {
            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.Decimal);
            parameter.Value = EnsureDBNull(value);

            return parameter;
        }

        public static MySqlParameter CreateDoubleParameter(double? value, string parameterName)
        {
            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.Double);
            parameter.Value = EnsureDBNull(value);

            return parameter;

        }

        public static MySqlParameter CreateInt32Parameter(int? value, string parameterName)
        {
            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.Int32, 11);
            parameter.DbType = DbType.Int32;
            parameter.Value = EnsureDBNull(value);

            return parameter;
        }

        public static MySqlParameter CreateBooleanParameter(bool? value, string parameterName)
        {
            int? bitValue = null;
            if (null != value)
                bitValue = value.Value == true ? 1 : 0;

            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.Bit);
            parameter.Value = EnsureDBNull(bitValue);

            return parameter;
        }

        public static MySqlParameter CreateStringParameter(string value, string parameterName)
        {
            MySqlParameter parameter = new MySqlParameter(parameterName, MySqlDbType.VarChar);
            parameter.Value = EnsureDBNull(value);

            return parameter;
        }

        public static MySqlParameter CreateNullParameter(string parameterName)
        {
            MySqlParameter parameter = new MySqlParameter(parameterName, DBNull.Value);

            return parameter;
        }

        public Guid GetGuid(MySqlDataReader reader, int ordinal)
        {
            byte[] byteBuffer = new byte[16];
            reader.GetBytes(ordinal, 0, byteBuffer, 0, 16);
            return new Guid(byteBuffer);
        }


        private static object EnsureDBNull(object value)
        {
            if (null == value)
                return DBNull.Value;
            return value;
        }
    }
}
