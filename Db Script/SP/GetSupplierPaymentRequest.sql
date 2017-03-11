USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[GetSupplierPaymentRequest]    Script Date: 2017-03-11 10:17:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetSupplierPaymentRequest] 
	@UserName as nvarchar(50)
AS
BEGIN
	
	Declare @RoalID as int
	Declare @ZoneID as nvarchar(15)
	Declare @ProvinceID as int

	SET NOCOUNT Off;

	SELECT @RoalID = RoleID, @ZoneID = ZoneID, @ProvinceID = ProvinceID FROM U_Users WHERE(UserName = @UserName)

	SET NOCOUNT ON;

		IF @RoalID = 1
		Begin
			

				SELECT   SupplierPaymentReq_Header.PaymentReqNo, SupplierPaymentReq_Header.ReqDate,SupplierPaymentReq_Header.TotalAmount, SupplierPaymentReq_Header.ProvinceID, SupplierPaymentReq_Header.Year, SupplierPaymentReq_Header.Month, 
                         SupplierPaymentReq_Header.CreateUser, SupplierPaymentReq_Header.CreateDate, SupplierPaymentReq_Header.AprovedUser, SupplierPaymentReq_Header.Status, m_Zones.ZoneName, 
                         m_Provinces.ProvinceName, SupplierPaymentReq_Header.ZoneID
				FROM            SupplierPaymentReq_Header LEFT OUTER JOIN
                         m_Provinces ON SupplierPaymentReq_Header.ProvinceID = m_Provinces.ProvinceID LEFT OUTER JOIN
                         m_Zones ON SupplierPaymentReq_Header.ZoneID = m_Zones.ZoneID


		End
		else if @RoalID = 2
		Begin

			SELECT SupplierPaymentReq_Header.PaymentReqNo, SupplierPaymentReq_Header.ReqDate,SupplierPaymentReq_Header.TotalAmount, SupplierPaymentReq_Header.ProvinceID, SupplierPaymentReq_Header.Year, SupplierPaymentReq_Header.Month, 
                   SupplierPaymentReq_Header.CreateUser, SupplierPaymentReq_Header.CreateDate, SupplierPaymentReq_Header.AprovedUser, SupplierPaymentReq_Header.Status, m_Zones.ZoneName, 
                   m_Provinces.ProvinceName, SupplierPaymentReq_Header.ZoneID
			FROM SupplierPaymentReq_Header LEFT OUTER JOIN m_Provinces ON 
				SupplierPaymentReq_Header.ProvinceID = m_Provinces.ProvinceID LEFT OUTER JOIN
                m_Zones ON SupplierPaymentReq_Header.ZoneID = m_Zones.ZoneID
			WHERE        (SupplierPaymentReq_Header.ProvinceID = @ProvinceID)

		End
		else if @RoalID = 3 OR @RoalID = 4
		Begin

			SELECT SupplierPaymentReq_Header.PaymentReqNo, SupplierPaymentReq_Header.ReqDate, SupplierPaymentReq_Header.TotalAmount, SupplierPaymentReq_Header.ProvinceID, SupplierPaymentReq_Header.Year, SupplierPaymentReq_Header.Month, 
                   SupplierPaymentReq_Header.CreateUser, SupplierPaymentReq_Header.CreateDate, SupplierPaymentReq_Header.AprovedUser, SupplierPaymentReq_Header.Status, m_Zones.ZoneName, 
                   m_Provinces.ProvinceName, SupplierPaymentReq_Header.ZoneID
			FROM SupplierPaymentReq_Header LEFT OUTER JOIN
				m_Provinces ON SupplierPaymentReq_Header.ProvinceID = m_Provinces.ProvinceID LEFT OUTER JOIN
                m_Zones ON SupplierPaymentReq_Header.ZoneID = m_Zones.ZoneID
			WHERE (SupplierPaymentReq_Header.ZoneID = @ZoneID)

		
		End


END

-- EXEC GetSupplierPaymentRquest 'ADMIN'