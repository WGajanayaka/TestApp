USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[LoadSuppliers]    Script Date: 2017-03-11 12:13:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[LoadSuppliers] 
	@ProvinceID as int, @ZoneID as nvarchar(10), @Year as int, @Month as nvarchar(15), @PaymentReqNo as Decimal(18,2)
AS
BEGIN

	Declare @ZoneIDNEW as int
	Declare @Status as nvarchar(15)
	
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	
	SET NOCOUNT OFF;
	
	SELECT @Status = Status FROM SupplierPaymentReq_Header WHERE PaymentReqNo = @PaymentReqNo
	IF @@rowcount = 0
	Begin
	
		Print 'No Records'

		SET NOCOUNT ON;
			SELECT        m_SupplierInformation.ID, @Status AS Status, m_Schools.CensorsID, m_SupplierInformation.SupplierName, m_SupplierInformation.BankAccountNo, m_Banks.BankName, m_BanksBranch.BranchName, 
									 m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, m_Banks.BankCode, m_BanksBranch.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, 
									 dbo.m_SupplierInformation.ID, 0 AS Amount
			FROM            dbo.m_SupplierInformation RIGHT OUTER JOIN
									 dbo.m_Schools ON dbo.m_SupplierInformation.SchoolID = dbo.m_Schools.SchoolID LEFT OUTER JOIN
									 dbo.m_Banks LEFT OUTER JOIN
									 dbo.m_BanksBranch ON dbo.m_Banks.ID = dbo.m_BanksBranch.BankID ON dbo.m_SupplierInformation.BankBranchID = dbo.m_BanksBranch.ID AND 
									 dbo.m_SupplierInformation.BankID = dbo.m_Banks.ID LEFT OUTER JOIN
									 dbo.Maximum_PayableAmount_For_Scool ON dbo.m_Schools.SchoolID = dbo.Maximum_PayableAmount_For_Scool.SchoolID
			WHERE        (m_Schools.ProvinceID = @ProvinceID) AND (m_Schools.ZoneID =@ZoneID) AND (m_SupplierInformation.Status = N'NEW') AND (Maximum_PayableAmount_For_Scool.Year = @Year) AND 
									 (dbo.m_SupplierInformation.ID NOT IN
										 (SELECT        SupplierID
										   FROM            dbo.SupplierPaymentReq_Details
										   WHERE        (Year = @Year) AND (Month = @Month) AND (dbo.m_Schools.ProvinceID = @ProvinceID) AND (dbo.m_Schools.ZoneID = @ZoneID)))
			ORDER BY dbo.m_Schools.CensorsID

		End
	ELSE
		Begin

		IF @Status = 'New'
		Begin

			SET NOCOUNT ON;

			/*

			SELECT        dbo.m_SupplierInformation.ID, dbo.SupplierPaymentReq_Details.CensusID AS CensorsID, dbo.m_SupplierInformation.SupplierName, dbo.SupplierPaymentReq_Details.AccountNo AS BankAccountNo, 
                         dbo.m_Banks.BankName, dbo.m_BanksBranch.BranchName, dbo.m_SupplierInformation.BankID, dbo.m_SupplierInformation.BankBranchID, dbo.SupplierPaymentReq_Details.BankCode, 
                         dbo.SupplierPaymentReq_Details.BranchCode, dbo.Maximum_PayableAmount_For_Scool.MAXAmount, dbo.m_SupplierInformation.ID AS Expr1, dbo.SupplierPaymentReq_Details.Amount
FROM            dbo.SupplierPaymentReq_Details INNER JOIN
                         dbo.Maximum_PayableAmount_For_Scool ON dbo.SupplierPaymentReq_Details.Year = dbo.Maximum_PayableAmount_For_Scool.Year LEFT OUTER JOIN
                         dbo.m_BanksBranch ON dbo.SupplierPaymentReq_Details.BankCode = dbo.m_BanksBranch.BankCode AND dbo.SupplierPaymentReq_Details.BranchCode = dbo.m_BanksBranch.BranchCode LEFT OUTER JOIN
                         dbo.m_Banks ON dbo.SupplierPaymentReq_Details.BankCode = dbo.m_Banks.BankCode RIGHT OUTER JOIN
                         dbo.m_SupplierInformation ON dbo.Maximum_PayableAmount_For_Scool.SchoolID = dbo.m_SupplierInformation.SchoolID AND 
                         dbo.SupplierPaymentReq_Details.SupplierID = dbo.m_SupplierInformation.ID
WHERE        (dbo.SupplierPaymentReq_Details.PaymentReqNo = 2)

			*/

			SELECT        m_SupplierInformation.ID, @Status AS Status, SupplierPaymentReq_Details.CensusID as CensorsID, m_SupplierInformation.SupplierName, SupplierPaymentReq_Details.AccountNo as BankAccountNo, 
						m_Banks.BankName, m_BanksBranch.BranchName, m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, SupplierPaymentReq_Details.BankCode, 
						SupplierPaymentReq_Details.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, m_SupplierInformation.ID, SupplierPaymentReq_Details.Amount
			FROM            SupplierPaymentReq_Details INNER JOIN
                        Maximum_PayableAmount_For_Scool ON SupplierPaymentReq_Details.Year = Maximum_PayableAmount_For_Scool.Year LEFT OUTER JOIN
                         m_BanksBranch ON SupplierPaymentReq_Details.BankCode = m_BanksBranch.BankCode AND SupplierPaymentReq_Details.BranchCode = m_BanksBranch.BranchCode LEFT OUTER JOIN
                         m_Banks ON SupplierPaymentReq_Details.BankCode = m_Banks.BankCode RIGHT OUTER JOIN
                         m_SupplierInformation ON Maximum_PayableAmount_For_Scool.SchoolID = m_SupplierInformation.SchoolID AND 
                         SupplierPaymentReq_Details.SupplierID = m_SupplierInformation.ID
			WHERE        (SupplierPaymentReq_Details.PaymentReqNo = @PaymentReqNo)
			UNION
			SELECT         m_SupplierInformation.ID,@Status AS Status, m_Schools.CensorsID as CensorsID, m_SupplierInformation.SupplierName, m_SupplierInformation.BankAccountNo, m_Banks.BankName, m_BanksBranch.BranchName, 
									 m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, m_Banks.BankCode, m_BanksBranch.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, 
									 dbo.m_SupplierInformation.ID, 0 AS Amount
			FROM            dbo.m_SupplierInformation RIGHT OUTER JOIN
									 dbo.m_Schools ON dbo.m_SupplierInformation.SchoolID = dbo.m_Schools.SchoolID LEFT OUTER JOIN
									 dbo.m_Banks LEFT OUTER JOIN
									 dbo.m_BanksBranch ON dbo.m_Banks.ID = dbo.m_BanksBranch.BankID ON dbo.m_SupplierInformation.BankBranchID = dbo.m_BanksBranch.ID AND 
									 dbo.m_SupplierInformation.BankID = dbo.m_Banks.ID LEFT OUTER JOIN
									 dbo.Maximum_PayableAmount_For_Scool ON dbo.m_Schools.SchoolID = dbo.Maximum_PayableAmount_For_Scool.SchoolID
			WHERE        (m_Schools.ProvinceID = @ProvinceID) AND (m_Schools.ZoneID =@ZoneID) AND (m_SupplierInformation.Status = N'NEW') AND (Maximum_PayableAmount_For_Scool.Year = @Year) AND 
									 (dbo.m_SupplierInformation.ID NOT IN
										 (SELECT        SupplierID
										   FROM            dbo.SupplierPaymentReq_Details
										   WHERE        (Year = @Year) AND (Month = @Month)))
			ORDER BY CensorsID

		End	
		Else
		Begin 

			SELECT        m_SupplierInformation.ID,@Status AS Status, SupplierPaymentReq_Details.CensusID as CensorsID, m_SupplierInformation.SupplierName, SupplierPaymentReq_Details.AccountNo as BankAccountNo, 
						m_Banks.BankName, m_BanksBranch.BranchName, m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, SupplierPaymentReq_Details.BankCode, 
						SupplierPaymentReq_Details.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, m_SupplierInformation.ID, SupplierPaymentReq_Details.Amount
			FROM            SupplierPaymentReq_Details INNER JOIN
                        Maximum_PayableAmount_For_Scool ON SupplierPaymentReq_Details.Year = Maximum_PayableAmount_For_Scool.Year LEFT OUTER JOIN
                         m_BanksBranch ON SupplierPaymentReq_Details.BankCode = m_BanksBranch.BankCode AND SupplierPaymentReq_Details.BranchCode = m_BanksBranch.BranchCode LEFT OUTER JOIN
                         m_Banks ON SupplierPaymentReq_Details.BankCode = m_Banks.BankCode RIGHT OUTER JOIN
                         m_SupplierInformation ON Maximum_PayableAmount_For_Scool.SchoolID = m_SupplierInformation.SchoolID AND 
                         SupplierPaymentReq_Details.SupplierID = m_SupplierInformation.ID
			WHERE        (SupplierPaymentReq_Details.PaymentReqNo = @PaymentReqNo)


		End
		
											   /*
		


			SELECT       SupplierPaymentReq_Details.CensusID, m_SupplierInformation.SupplierName, SupplierPaymentReq_Details.AccountNo, m_Banks.BankName, m_BanksBranch.BranchName, 
						m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, SupplierPaymentReq_Details.BankCode, SupplierPaymentReq_Details.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, 
						m_SupplierInformation.ID, SupplierPaymentReq_Details.Amount 
				FROM            SupplierPaymentReq_Details INNER JOIN
                         m_Banks ON SupplierPaymentReq_Details.BankCode = m_Banks.BankCode INNER JOIN
                         m_BanksBranch ON SupplierPaymentReq_Details.BankCode = m_BanksBranch.BankCode AND SupplierPaymentReq_Details.BranchCode = m_BanksBranch.BranchCode INNER JOIN
                         Maximum_PayableAmount_For_Scool ON SupplierPaymentReq_Details.Year = Maximum_PayableAmount_For_Scool.Year RIGHT OUTER JOIN
                         m_SupplierInformation ON Maximum_PayableAmount_For_Scool.SchoolID = m_SupplierInformation.SchoolID AND SupplierPaymentReq_Details.SupplierID = m_SupplierInformation.ID
			WHERE        (SupplierPaymentReq_Details.PaymentReqNo = 1)
			UNION
			SELECT         m_Schools.CensorsID, m_SupplierInformation.SupplierName, m_SupplierInformation.BankAccountNo as AccountNo, m_Banks.BankName, m_BanksBranch.BranchName, 
						m_SupplierInformation.BankID, m_SupplierInformation.BankBranchID, m_Banks.BankCode, m_BanksBranch.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount, 
									 dbo.m_SupplierInformation.ID, 0 AS Amount
			FROM            dbo.m_SupplierInformation RIGHT OUTER JOIN
									 dbo.m_Schools ON dbo.m_SupplierInformation.SchoolID = dbo.m_Schools.SchoolID LEFT OUTER JOIN
									 dbo.m_Banks LEFT OUTER JOIN
									 dbo.m_BanksBranch ON dbo.m_Banks.ID = dbo.m_BanksBranch.BankID ON dbo.m_SupplierInformation.BankBranchID = dbo.m_BanksBranch.ID AND 
									 dbo.m_SupplierInformation.BankID = dbo.m_Banks.ID LEFT OUTER JOIN
									 dbo.Maximum_PayableAmount_For_Scool ON dbo.m_Schools.SchoolID = dbo.Maximum_PayableAmount_For_Scool.SchoolID
			WHERE        (m_Schools.ProvinceID = 5) AND (m_Schools.ZoneID = '1606') AND (m_SupplierInformation.Status = N'NEW') AND (Maximum_PayableAmount_For_Scool.Year = 2016) AND 
									 (dbo.m_SupplierInformation.ID NOT IN
										 (SELECT        SupplierID
										   FROM            dbo.SupplierPaymentReq_Details
										   WHERE        (Year = 2016) AND (Month = 'January')))


										   */



		End

END


-- exec LoadSuppliers 1, '0103' , 2016, 'January', 2

-- exec LoadSuppliers

-- EXEC  LoadSuppliers 8, '2203', 2017

