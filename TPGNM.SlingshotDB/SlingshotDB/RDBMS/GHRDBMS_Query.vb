﻿Imports Grasshopper
Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports GH_IO
Imports GH_IO.Serialization

Imports System

Public Class GHRDBMS_Query
  Inherits Grasshopper.Kernel.GH_Component

  Private _rdbms As String = "MySQL"

#Region "Register"
  'Methods
  Public Sub New()
    MyBase.New("RDBMS Query", "Query", "Send a query to a Relational Database Management System", "Slingshot!", "RDBMS")
  End Sub

  'Exposure parameter (line dividers)
  Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
    Get
      Return GH_Exposure.tertiary
    End Get
  End Property

  'GUID generator http://www.guidgenerator.com/online-guid-generator.aspx
  Public Overrides ReadOnly Property ComponentGuid As System.Guid
    Get
      Return New Guid("{99fefdaa-28ca-44ef-9996-13fbed4bf0ab}")
    End Get
  End Property

  'Icon 24x24
  Protected Overrides ReadOnly Property Internal_Icon_24x24 As System.Drawing.Bitmap
    Get
      Return My.Resources.GHMySQL_Query
    End Get
  End Property
#End Region

#Region "Menu Items"
  'Append Component menues.
  Public Overrides Function AppendMenuItems(menu As Windows.Forms.ToolStripDropDown) As Boolean

    Menu_AppendItem(menu, "Connector Settings...", AddressOf Menu_Settings)

    Return True
  End Function

  'On menu item click...
  Private Sub Menu_Settings(ByVal sender As Object, ByVal e As EventArgs)

    'Open Settings dialogue
    Dim m_settingsdialogue As New form_DBConnector(_rdbms)
    m_settingsdialogue.ShowDialog()
    _rdbms = m_settingsdialogue.Connector

    ExpireSolution(True)

  End Sub

  'GH Writer
  Public Overrides Function Write(writer As GH_IWriter) As Boolean
    writer.SetString("Connector", _rdbms)
    Return MyBase.Write(writer)
  End Function

  'GH Reader
  Public Overrides Function Read(reader As GH_IReader) As Boolean
    reader.TryGetString("Connector", _rdbms)
    Return MyBase.Read(reader)
  End Function
#End Region

#Region "Inputs/Outputs"

  Protected Overrides Sub RegisterInputParams(ByVal pManager As Grasshopper.Kernel.GH_Component.GH_InputParamManager)
    pManager.AddBooleanParameter("Connect Toggle", "CToggle", "Set to 'True' to connect.", False, GH_ParamAccess.item)
    pManager.AddTextParameter("Connect String", "CString", "A MySQL connection string.", GH_ParamAccess.item)
    pManager.AddTextParameter("RDBMS Query", "Query", "A SQL query.", GH_ParamAccess.item)
    pManager.AddIntegerParameter("Column Number", "Column", "The column number to output.", 0, GH_ParamAccess.item)
  End Sub

#End Region

#Region "Solution"

  Protected Overrides Sub RegisterOutputParams(ByVal pManager As Grasshopper.Kernel.GH_Component.GH_OutputParamManager)
    pManager.Register_GenericParam("Exceptions", "out", "Displays errors.")
    pManager.Register_GenericParam("Column Query Result", "CResult", "Results in a specific column")
    pManager.Register_GenericParam("Query Result", "QResult", "Full result of a query.  Columns separated by commas.")
  End Sub

  Protected Overrides Sub SolveInstance(ByVal DA As Grasshopper.Kernel.IGH_DataAccess)
    Try
      Dim RDBMS As String = _rdbms
      Dim cstring As String = Nothing
      Dim connect As Boolean = False
      Dim query As String = Nothing
      Dim column As Integer = Nothing

      DA.GetData(Of String)(0, cstring)
      DA.GetData(Of Boolean)(1, connect)
      DA.GetData(Of String)(2, query)
      DA.GetData(Of Integer)(3, column)

      If connect = True Then
        Dim sqlDataSet As DataSet = Nothing

        Dim dbcommand As New clsRDBMS()
        If RDBMS = "MySQL" Then
          sqlDataSet = dbcommand.MySQLQuery(cstring, query)
        ElseIf RDBMS = "ODBC" Then
          sqlDataSet = dbcommand.ODBCQuery(cstring, query)
        ElseIf RDBMS = "OLEDB" Then
          sqlDataSet = dbcommand.OLEDBQuery(cstring, query)
        End If

        'Get data lists
        Dim DataListA As New List(Of Object)
        For i As Integer = 0 To sqlDataSet.Tables(0).Rows.Count - 1
          DataListA.Add(sqlDataSet.Tables(0).Rows(i)(column))
        Next

        Dim DataListB As New List(Of Object)
        For i As Integer = 0 To sqlDataSet.Tables(0).Rows.Count - 1
          Dim rowString As String = sqlDataSet.Tables(0).Rows(i)(0)
          For j As Integer = 1 To sqlDataSet.Tables(0).Columns.Count - 1
            rowString = rowString & "," & sqlDataSet.Tables(0).Rows(i)(j)
          Next
          DataListB.Add(rowString)
        Next

        'Set Data lists to outputs
        DA.SetDataList(1, DataListA)
        DA.SetDataList(2, DataListB)
      End If

    Catch ex As Exception
      DA.SetData(0, ex.ToString)
    End Try

  End Sub

#End Region

End Class
