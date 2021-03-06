Imports System.IO
Imports System.Data
Imports System.Drawing
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Globalization

Public Class bmsetara_calon_baru_daftar
    Inherits System.Web.UI.UserControl
    Dim oCommon As New Commonfunction
    Dim strSQL As String = ""
    Dim strRet As String = ""
    Dim IntTakwim As Integer = 0

    Dim strConn As String = ConfigurationManager.AppSettings("ConnectionString")
    Dim objConn As SqlConnection = New SqlConnection(strConn)
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Not Page.IsPostBack Then

                kpmkv_tahun_list()
                ddlTahun.Enabled = False

                kpmkv_semester_list()
                ddlSemester.Enabled = False

                chkSesi.Enabled = False

                'kolejnama
                strSQL = "SELECT Nama FROM kpmkv_users WHERE LoginID='" & Session("LoginID") & "'"
                Dim strKolejnama As String = oCommon.getFieldValue(strSQL)

                'kolejid
                strSQL = "SELECT RecordID FROM kpmkv_kolej WHERE Nama='" & strKolejnama & "'"
                lblKolejID.Text = oCommon.getFieldValue(strSQL)

                '------exist takwim
                strSQL = "SELECT * FROM kpmkv_takwim WHERE Tahun='" & Now.Year & "' AND SubMenuText = 'Daftar Calon Baru' AND Aktif='1'"
                If oCommon.isExist(strSQL) = True Then

                    'count data takwim
                    'Get the data from database into datatable
                    Dim cmd As New SqlCommand("SELECT TakwimID FROM kpmkv_takwim WHERE Tahun='" & Now.Year & "' AND SubMenuText = 'Daftar Calon Baru' AND Aktif='1'")
                    Dim dt As DataTable = GetData(cmd)

                    For i As Integer = 0 To dt.Rows.Count - 1
                        IntTakwim = dt.Rows(i)("TakwimID")

                        strSQL = "SELECT TarikhMula,TarikhAkhir FROM kpmkv_takwim WHERE TakwimID='" & IntTakwim & "'"
                        strRet = oCommon.getFieldValueEx(strSQL)

                        Dim ar_user_login As Array
                        ar_user_login = strRet.Split("|")
                        Dim strMula As String = ar_user_login(0)
                        Dim strAkhir As String = ar_user_login(1)

                        Dim strdateNow As Date = Date.Now
                        Dim startDate = DateTime.ParseExact(strMula, "dd-MM-yyyy", CultureInfo.InvariantCulture)
                        Dim endDate = DateTime.ParseExact(strAkhir, "dd-MM-yyyy", CultureInfo.InvariantCulture)

                        Dim ts As New TimeSpan
                        ts = endDate.Subtract(strdateNow)
                        Dim dayDiff = ts.Days

                        If strMula IsNot Nothing Then
                            If strAkhir IsNot Nothing And dayDiff >= 0 Then

                                ddlTahun.Enabled = True
                                ddlTahun.Text = Now.Year

                                ddlSemester.Enabled = True
                                ddlSemester.Text = ""

                                chkSesi.Enabled = True


                                kpmkv_kelas_list()

                                kpmkv_kodkursus_list()

                                'checkinbox
                                strSQL = "SELECT Sesi FROM kpmkv_takwim WHERE TakwimId='" & IntTakwim & "'ORDER BY Kohort ASC"
                                strRet = oCommon.getFieldValue(strSQL)

                                If strRet = 1 Then
                                    chkSesi.Items(0).Enabled = True
                                    ' chkSesi.Items(1).Enabled = False
                                Else
                                    ' chkSesi.Items(0).Enabled = False
                                    chkSesi.Items(1).Enabled = True
                                End If
                                btnCari.Enabled = True
                                btnConfirm.Enabled = True
                            End If
                        Else
                            btnCari.Enabled = False
                            btnConfirm.Enabled = False
                            lblMsg.Text = "Daftar Calon Baru telah ditutup!"
                        End If
                    Next
                Else
                    btnCari.Enabled = False
                    btnConfirm.Enabled = False
                    lblMsg.Text = "Daftar Calon Baru telah ditutup!"
                End If
                'RepoveDuplicate(ddlTahun)
                'RepoveDuplicate(ddlsemester)
            End If


        Catch ex As Exception
            lblMsg.Text = ex.Message
        End Try
    End Sub
    Private Sub kpmkv_tahun_list()
        strSQL = "  SELECT DISTINCT Kohort FROM kpmkv_takwim
                    WHERE '" & Format(CDate(Date.Now), "dd-MM-yyyy") & "' BETWEEN TarikhMula AND TarikhAkhir"

        Dim strConn As String = ConfigurationManager.AppSettings("ConnectionString")
        Dim objConn As SqlConnection = New SqlConnection(strConn)
        Dim sqlDA As New SqlDataAdapter(strSQL, objConn)

        Try
            Dim ds As DataSet = New DataSet
            sqlDA.Fill(ds, "AnyTable")

            ddlTahun.DataSource = ds
            ddlTahun.DataTextField = "Kohort"
            ddlTahun.DataValueField = "Kohort"
            ddlTahun.DataBind()

            ddlTahun.Items.Add(New ListItem("-Pilih-", ""))

        Catch ex As Exception

            lblMsg.Text = "System Error:" & ex.Message

        Finally
            objConn.Dispose()
        End Try
    End Sub

    Private Sub kpmkv_semester_list()
        strSQL = "  SELECT DISTINCT Semester FROM kpmkv_takwim
                    WHERE '" & Format(CDate(Date.Now), "dd-MM-yyyy") & "' BETWEEN TarikhMula AND TarikhAkhir"
        Dim strConn As String = ConfigurationManager.AppSettings("ConnectionString")
        Dim objConn As SqlConnection = New SqlConnection(strConn)
        Dim sqlDA As New SqlDataAdapter(strSQL, objConn)

        Try
            Dim ds As DataSet = New DataSet
            sqlDA.Fill(ds, "AnyTable")

            ddlSemester.DataSource = ds
            ddlSemester.DataTextField = "Semester"
            ddlSemester.DataValueField = "Semester"
            ddlSemester.DataBind()

            ddlSemester.Items.Add(New ListItem("-Pilih-", ""))

        Catch ex As Exception
            lblMsg.Text = "System Error:" & ex.Message

        Finally
            objConn.Dispose()
        End Try

    End Sub

    Private Sub kpmkv_kodkursus_list()

        strSQL = "SELECT kpmkv_kursus.KodKursus, kpmkv_kursus.KursusID FROM kpmkv_kursus_kolej LEFT OUTER JOIN"
        strSQL += " kpmkv_kursus ON kpmkv_kursus_kolej.KursusID = kpmkv_kursus.KursusID"
        strSQL += " WHERE kpmkv_kursus_kolej.KolejRecordID='" & lblKolejID.Text & "' AND kpmkv_kursus.Tahun='" & ddlTahun.SelectedValue & "' "
        strSQL += " AND kpmkv_kursus.Sesi='" & chkSesi.SelectedValue & "' GROUP BY kpmkv_kursus.KodKursus, kpmkv_kursus.KursusID ORDER BY kpmkv_kursus.KodKursus"
        Dim strConn As String = ConfigurationManager.AppSettings("ConnectionString")
        Dim objConn As SqlConnection = New SqlConnection(strConn)
        Dim sqlDA As New SqlDataAdapter(strSQL, objConn)

        Try
            Dim ds As DataSet = New DataSet
            sqlDA.Fill(ds, "AnyTable")

            ddlKodKursus.DataSource = ds
            ddlKodKursus.DataTextField = "KodKursus"
            ddlKodKursus.DataValueField = "KursusID"
            ddlKodKursus.DataBind()


        Catch ex As Exception
            lblMsg.Text = "System Error:" & ex.Message

        Finally
            objConn.Dispose()
        End Try

    End Sub

    Private Sub kpmkv_kelas_list()
        strSQL = " SELECT kpmkv_kelas.NamaKelas, kpmkv_kelas.KelasID"
        strSQL += " FROM  kpmkv_kelas_kursus LEFT OUTER JOIN kpmkv_kelas ON kpmkv_kelas_kursus.KelasID = kpmkv_kelas.KelasID LEFT OUTER JOIN"
        strSQL += " kpmkv_kursus ON kpmkv_kelas_kursus.KursusID = kpmkv_kursus.KursusID"
        strSQL += " WHERE kpmkv_kelas.KolejRecordID='" & lblKolejID.Text & "' AND kpmkv_kelas_kursus.KursusID= '" & ddlKodKursus.SelectedValue & "' ORDER BY  kpmkv_kelas.NamaKelas"
        Dim strConn As String = ConfigurationManager.AppSettings("ConnectionString")
        Dim objConn As SqlConnection = New SqlConnection(strConn)
        Dim sqlDA As New SqlDataAdapter(strSQL, objConn)

        Try
            Dim ds As DataSet = New DataSet
            sqlDA.Fill(ds, "AnyTable")

            ddlNamaKelas.DataSource = ds
            ddlNamaKelas.DataTextField = "NamaKelas"
            ddlNamaKelas.DataValueField = "KelasID"
            ddlNamaKelas.DataBind()


        Catch ex As Exception
            lblMsg.Text = "System Error:" & ex.Message

        Finally
            objConn.Dispose()
        End Try

    End Sub

    Private Sub Year()

        For i As Integer = ddlTahun.Text To Now.Year
            ddlTahunSemasa.Items.Add(i.ToString())
        Next
        ddlTahunSemasa.Items.FindByValue(System.DateTime.Now.Year.ToString()).Selected = True

    End Sub
    Private Function BindData(ByVal gvTable As GridView) As Boolean
        Dim myDataSet As New DataSet
        Dim myDataAdapter As New SqlDataAdapter(getSQL, strConn)
        myDataAdapter.SelectCommand.CommandTimeout = 120

        Try
            myDataAdapter.Fill(myDataSet, "myaccount")

            If myDataSet.Tables(0).Rows.Count = 0 Then
                divMsg.Attributes("class") = "error"
                lblMsg.Text = "Rekod tidak dijumpai!"
            Else
                divMsg.Attributes("class") = "info"
                lblMsg.Text = "Jumlah Rekod#:" & myDataSet.Tables(0).Rows.Count
            End If

            gvTable.DataSource = myDataSet
            gvTable.DataBind()
            objConn.Close()
        Catch ex As Exception
            lblMsg.Text = "System Error:" & ex.Message
            Return False
        End Try

        Return True

    End Function

    Private Function getSQL() As String
        Dim tmpSQL As String
        Dim strWhere As String = ""
        Dim strOrder As String = " ORDER BY kpmkv_pelajar.Nama ASC"

        '--not deleted
        tmpSQL = "SELECT kpmkv_pelajar.PelajarID, kpmkv_pelajar.Tahun, kpmkv_pelajar.Semester, kpmkv_pelajar.Sesi, "
        tmpSQL += " kpmkv_pelajar.Nama, kpmkv_pelajar.MYKAD, kpmkv_pelajar.AngkaGiliran, "
        tmpSQL += " kpmkv_kursus.KodKursus"
        tmpSQL += " FROM  kpmkv_pelajar "
        tmpSQL += " LEFT OUTER JOIN kpmkv_kursus ON kpmkv_pelajar.KursusID = kpmkv_kursus.KursusID"
        tmpSQL += " LEFT OUTER JOIN kpmkv_kluster ON kpmkv_kursus.KlusterID=kpmkv_kluster.KlusterID"
        tmpSQL += " LEFT OUTER JOIN kpmkv_status ON kpmkv_pelajar.StatusID = kpmkv_status.StatusID "
        tmpSQL += " LEFT OUTER JOIN kpmkv_kelas ON kpmkv_pelajar.KelasID = kpmkv_kelas.KelasID"
        strWhere = " WHERE kpmkv_pelajar.IsDeleted='N' AND kpmkv_pelajar.StatusID='2' "
        strWhere += " AND kpmkv_pelajar.KolejRecordID='" & lblKolejID.Text & "'"
        strWhere += " AND kpmkv_pelajar.Tahun ='" & ddlTahun.Text & "' "
        strWhere += " AND kpmkv_pelajar.Semester ='" & ddlSemester.SelectedValue & "'"
        strWhere += " AND kpmkv_pelajar.Sesi='" & chkSesi.Text & "'"

        '--kodkursus
        If Not ddlKodKursus.Text = "" Then
            strWhere += " AND kpmkv_pelajar.KursusID ='" & ddlKodKursus.SelectedValue & "'"
        End If

        '--txtNama
        If Not txtNama.Text.Length = 0 Then
            strWhere += " AND kpmkv_pelajar.Nama LIKE '%" & oCommon.FixSingleQuotes(txtNama.Text) & "%'"
        End If

        '--txtMYKAD
        If Not txtMYKAD.Text.Length = 0 Then
            strWhere += " AND kpmkv_pelajar.MYKAD='" & oCommon.FixSingleQuotes(txtMYKAD.Text) & "'"
        End If

        getSQL = tmpSQL & strWhere & strOrder
        ''--debug
        'Response.Write(getSQL)



        Return getSQL

    End Function

    Private Sub status_daftar()

        For i = 0 To datRespondent2.Rows.Count - 1

            Dim strkey As String = datRespondent2.DataKeys(i).Value.ToString

            Dim lblstatusBM As Label = datRespondent2.Rows(i).FindControl("lblStatusBM")
            Dim lblstatusSJ As Label = datRespondent2.Rows(i).FindControl("lblStatusSJ")


            strSQL = "SELECT isCalon FROM kpmkv_pelajar WHERE PelajarID = '" & strkey & "'"
            Dim statusBM As String = oCommon.getFieldValue(strSQL)

            strSQL = "SELECT IsSJCalon FROM kpmkv_pelajar WHERE PelajarID = '" & strkey & "'"
            Dim statusSJ As String = oCommon.getFieldValue(strSQL)

            If statusBM = "1" Then

                lblstatusBM.Text = "Telah Daftar"

            End If

            If statusSJ = "1" Then

                lblstatusSJ.Text = "Telah Daftar"

            End If

        Next

    End Sub

    Private Sub datRespondent_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles datRespondent2.PageIndexChanging
        datRespondent2.PageIndex = e.NewPageIndex
        strRet = BindData(datRespondent2)

    End Sub
    Private Function GetData(ByVal cmd As SqlCommand) As DataTable
        Dim dt As New DataTable()
        Dim strConnString As [String] = ConfigurationManager.AppSettings("ConnectionString")
        Dim con As New SqlConnection(strConnString)
        Dim sda As New SqlDataAdapter()
        cmd.CommandType = CommandType.Text
        cmd.Connection = con
        Try
            con.Open()
            sda.SelectCommand = cmd
            sda.Fill(dt)
            Return dt
        Catch ex As Exception
            Throw ex
        Finally
            con.Close()
            sda.Dispose()
            con.Dispose()
        End Try
    End Function

    Protected Sub OnConfirm(ByVal sender As Object, ByVal e As EventArgs) Handles btnConfirm.Click
        Dim confirmValue As String = Request.Form("confirm_value")
        If confirmValue = "Yes" Then
            returnconfirm()
            status_daftar()
        Else
            strRet = BindData(datRespondent2)
        End If
    End Sub

    Protected Sub CheckUncheckAll(sender As Object, e As System.EventArgs)
        Dim chk1 As CheckBox
        chk1 = DirectCast(datRespondent2.HeaderRow.Cells(0).FindControl("chkAll"), CheckBox)
        For Each row As GridViewRow In datRespondent2.Rows
            Dim chk As CheckBox
            chk = DirectCast(row.Cells(0).FindControl("chkSelect"), CheckBox)
            chk.Checked = chk1.Checked
        Next
    End Sub

    Private Sub returnconfirm()
        If ddlmp.SelectedValue = "" Then
            divMsg.Attributes("class") = "error"
            lblMsg.Text = "Sila Pilih Mata Pelajaran!"
            ddlmp.Focus()
            Exit Sub
        End If

        Try
            For i As Integer = 0 To datRespondent2.Rows.Count - 1

                Dim cb As CheckBox = datRespondent2.Rows(i).FindControl("chkSelect")

                If cb.Checked = True Then

                    Dim strkey As String = datRespondent2.DataKeys(i).Value.ToString


                    strSQL = " UPDATE kpmkv_pelajar SET "
                    If ddlmp.SelectedValue = "BM" Then

                        strSQL += " IsCalon ='1', IsBMTahun='" & ddlTahunSemasa.Text & "', "
                        strSQL += " IsBMDated ='" & Date.Now.ToString("yyyy/MM/dd") & "'"
                    ElseIf ddlmp.SelectedValue = "SJ" Then
                        strSQL += " IsSJCalon ='1', IsSJTahun='" & ddlTahunSemasa.Text & "', "
                        strSQL += " IsSJDated ='" & Date.Now.ToString("yyyy/MM/dd") & "'"
                    End If
                    strSQL += " WHERE PelajarID='" & strkey & "'"
                    strRet = oCommon.ExecuteSQL(strSQL)
                    If strRet = "0" Then
                        divMsg.Attributes("class") = "info"
                        lblMsg.Text = "Berjaya! Pengesahan Calon Baru Berjaya"

                        divMsgResult.Attributes("class") = "info"
                        lblMsgResult.Text = "Berjaya! Pengesahan Calon Baru Berjaya"
                    Else
                        divMsg.Attributes("class") = "error"
                        lblMsg.Text = "Tidak Berjaya! Pengesahan Calon Baru Tidak Berjaya"

                        divMsgResult.Attributes("class") = "error"
                        lblMsgResult.Text = "Tidak Berjaya! Pengesahan Calon Baru Tidak Berjaya"
                    End If
                End If

            Next
            divMsg.Attributes("class") = "info"
            lblMsg.Text = "Berjaya! Pengesahan Calon Baru  Berjaya"

            divMsgResult.Attributes("class") = "info"
            lblMsgResult.Text = "Berjaya! Pengesahan Calon Baru Berjaya"
        Catch ex As Exception
            divMsg.Attributes("class") = "error"
            lblMsg.Text = "System Error. " & ex.Message
        End Try

    End Sub

    Private Sub btnCari_Click(sender As Object, e As EventArgs) Handles btnCari.Click
        lblMsg.Text = ""
        strRet = BindData(datRespondent2)
        status_daftar()
        Year()
    End Sub

    Protected Sub chkSesi_SelectedIndexChanged(sender As Object, e As EventArgs) Handles chkSesi.SelectedIndexChanged
        kpmkv_kodkursus_list()
        kpmkv_kelas_list()
    End Sub

    Private Sub ddlKodKursus_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlKodKursus.SelectedIndexChanged
        kpmkv_kelas_list()
    End Sub


End Class