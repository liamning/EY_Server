use EYVoting
alter table ClientMaster
add OrganizationCode nvarchar(100)


alter table ClientMaster
add SocialUnifiedCreditCode nvarchar(100)


use EYVoting
alter table CreditorMaster
add HasSpecialCreditType bit


alter table CreditorMaster
add SpecialCreditTypeRemarks ntext


alter table CreditorMaster
add WarrantyRemarks ntext