namespace Investly.API.Models.Enums;

public enum UserRole { investor, owner, admin, guest }
public enum UserStatus { active, pending, suspended, banned }
public enum KycStatus { none, pending, approved, rejected }
public enum Gender { male, female, other }

public enum WalletStatus { active, frozen, inactive }
public enum WalletTransactionType { credit, debit }
public enum WalletTransactionStatus { completed, pending, failed, refunded }

public enum ProjectStatus { pending, active, completed, inactive, rejected }

public enum InvestmentStatus { pending, completed, failed, cancelled }
public enum PaymentMethod { wallet, credit_card }

public enum PaymentStatus { pending, completed, failed, refunded }

public enum NotificationType { investment, project, system, user }

public enum OtpPurpose { login, register, reset }
