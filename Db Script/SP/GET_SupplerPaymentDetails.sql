USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[GET_SupplerPaymentDetails]    Script Date: 2017-03-27 10:42:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GET_SupplerPaymentDetails]
	@Year as int, @Month as Nvarchar(15), @province as int
AS
BEGIN
	
			SELECT        m_Provinces.ProvinceName, m_Zones.ZoneName, SupplierPaymentReq_Header.PaymentReqNo, SupplierPaymentReq_Details.CensusID, m_SupplierInformation.SupplierName, m_SupplierInformation.ID AS SupplierId,
									 m_SupplierInformation.BankAccountNo, m_Banks.BankName, m_BanksBranch.BranchName, SupplierPaymentReq_Details.Amount, SupplierPaymentReq_Details.BankCode, 
									 SupplierPaymentReq_Details.BranchCode, SupplierPaymentReq_Header.PaymentReqNo AS supplierPaymentReq_HeaderId
			FROM            m_BanksBranch RIGHT OUTER JOIN
									 m_SupplierInformation INNER JOIN
									 SupplierPaymentReq_Details ON m_SupplierInformation.ID = SupplierPaymentReq_Details.SupplierID ON m_BanksBranch.ID = m_SupplierInformation.ID AND 
									 m_BanksBranch.BankCode = SupplierPaymentReq_Details.BankCode AND m_BanksBranch.BranchCode = SupplierPaymentReq_Details.BranchCode LEFT OUTER JOIN
									 m_Banks ON SupplierPaymentReq_Details.BankCode = m_Banks.BankCode RIGHT OUTER JOIN
									 m_Zones INNER JOIN
									 SupplierPaymentReq_Header ON m_Zones.ZoneID = SupplierPaymentReq_Header.ZoneID LEFT OUTER JOIN
									 m_Provinces ON SupplierPaymentReq_Header.ProvinceID = m_Provinces.ProvinceID ON SupplierPaymentReq_Details.PaymentReqNo = SupplierPaymentReq_Header.PaymentReqNo
			WHERE        (SupplierPaymentReq_Header.Status = N'Forwarded') AND (SupplierPaymentReq_Header.Year = @Year) AND (SupplierPaymentReq_Header.Month =  @Month) AND (SupplierPaymentReq_Header.ProvinceID = @province) AND (SupplierPaymentReq_Details.paid = 0)


END
