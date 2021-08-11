using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;
using GMap.NET.MapProviders;
using System.Globalization;

namespace GeopositioningTestTask
{
    public partial class mainForm : Form
    {
        SqlConnection sqlConnection;
        GMapMarker dragAndDropMarker;
        GMapOverlay markers;


        public mainForm()
        {
            InitializeComponent();
        }

        private void SetMapParameters()
        {
            map.MapProvider = GMapProviders.GoogleMap;
            map.MinZoom = 1;
            map.MaxZoom = 20;
            map.Zoom = 10;

            map.DragButton = MouseButtons.Left;
            map.OnMarkerClick += Map_OnMarkerClick;
            map.OnMapClick += Map_OnMapClick;

            map.Position = new PointLatLng(55, 83);
        }

        private void LoadPointsCoordinates()
        {
            string connctionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""E:\C# GoGo\GeopositioningTestTask\GeopositioningTestTask\Database.mdf"";Integrated Security=True";

            sqlConnection = new SqlConnection(connctionString);

            sqlConnection.Open();

            SqlDataReader sqlDataReader = null;

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM [Coordinates]", sqlConnection);

            try
            {
                sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    PointLatLng point = new PointLatLng(Convert.ToDouble(sqlDataReader["Latitude"]), Convert.ToDouble(sqlDataReader["Longitude"]));
                    GMapMarker marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
                    markers.Markers.Add(marker);
                }
                map.Overlays.Add(markers);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlDataReader != null)
                    sqlDataReader.Close();
            }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            markers = new GMapOverlay("markers");
            LoadPointsCoordinates();
            SetMapParameters();
        }

        private void Map_OnMapClick(PointLatLng pointClick, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && dragAndDropMarker != null)
            {
                dragAndDropMarker.Position = pointClick;
                dragAndDropMarker.Size = new Size(30, 30);
                dragAndDropMarker = null;
            }
        }

        private void Map_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && dragAndDropMarker == null)
            {
                dragAndDropMarker = item;
                dragAndDropMarker.Size = new Size(50, 50);
            }
        }

        private void SaveMarkersLocation()
        {
            SqlCommand sqlCommand;

            for (int i = 0; i < markers.Markers.Count; i++)
            {
                sqlCommand = new SqlCommand("UPDATE [Coordinates] SET [Latitude]=@Latitude, [Longitude]=@Longitude WHERE [Id]=@Id", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@Id", i + 1);
                sqlCommand.Parameters.AddWithValue("@Latitude", markers.Markers[i].Position.Lat);
                sqlCommand.Parameters.AddWithValue("@Longitude", markers.Markers[i].Position.Lng);
                sqlCommand.ExecuteNonQueryAsync();
            }
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveMarkersLocation();
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }
    }
}
