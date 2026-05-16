/**
 * RechargeWalletScreen.js — Wallet top-up with prepaid cards
 *
 * Flow:
 *   1. Load available recharge card options via investmentAPI.getFundingOptions()
 *   2. User selects a card denomination (sorted cheapest → most expensive)
 *   3. User enters the card code (auto-uppercased, forced LTR direction)
 *   4. Submit → investmentAPI.redeemTopupCard(code) → update wallet balance
 *
 * The code input is always LTR regardless of app language because card codes
 * are alphanumeric strings with no RTL meaning.
 */
import React, { useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  StatusBar,
  ActivityIndicator,
  TextInput,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';

import { COLORS, FONTS, RADIUS, SHADOWS, SPACING } from '../constants/theme';
import { investmentAPI } from '../services/api';
import { useAuth } from '../hooks/useAuth';
import { useTopPopup } from '../hooks/useTopPopup';

const formatCurrency = (value, isAr) => {
  const amount = Number(value || 0).toLocaleString('en-US');
  return isAr ? `${amount} د.ل` : `LYD ${amount}`;
};

const RechargeWalletScreen = ({ navigation }) => {
  const { i18n } = useTranslation();
  const isAr = i18n.language === 'ar';
  const insets = useSafeAreaInsets();
  const popup = useTopPopup();
  const { user, updateUser } = useAuth();

  const [wallet, setWallet] = useState({ balance: Number(user?.walletBalance || 0), totalTopups: Number(user?.totalTopups || 0), transactions: [] });
  const [fundingOptions, setFundingOptions] = useState({ methods: [], rechargeCards: [] });
  const [selectedCode, setSelectedCode] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  const sortedCards = useMemo(
    () => [...(fundingOptions.rechargeCards || [])].sort((a, b) => Number(a.amount || 0) - Number(b.amount || 0)),
    [fundingOptions.rechargeCards],
  );

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const [walletResponse, fundingResponse] = await Promise.all([
          investmentAPI.getWallet(),
          investmentAPI.getFundingOptions(),
        ]);

        const walletData = walletResponse?.data || walletResponse || {};
        const fundingData = fundingResponse?.data || fundingResponse || {};

        setWallet({
          balance: Number(walletData.balance || 0),
          totalTopups: Number(walletData.totalTopups || 0),
          transactions: walletData.transactions || [],
        });
        setFundingOptions({
          methods: fundingData.methods || [],
          rechargeCards: fundingData.rechargeCards || [],
        });
      } catch (error) {
        popup.error(error?.message || (isAr ? 'تعذر تحميل بيانات الشحن حالياً' : 'Unable to load top-up data right now'));
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [isAr, popup]);

  const handleRedeem = async () => {
    const normalizedCode = selectedCode.trim();
    if (!normalizedCode || submitting) return;

    setSubmitting(true);
    try {
      const response = await investmentAPI.redeemTopupCard(normalizedCode);
      const payload = response?.data || response || {};
      const nextBalance = Number(payload.balance || wallet.balance);
      const nextTotalTopups = Number(payload.totalTopups || wallet.totalTopups);

      setWallet((prev) => ({
        ...prev,
        balance: nextBalance,
        totalTopups: nextTotalTopups,
      }));

      await updateUser({
        walletBalance: nextBalance,
        totalTopups: nextTotalTopups,
      });

      popup.success(isAr ? 'تم شحن الرصيد بنجاح' : 'Wallet topped up successfully');
      setSelectedCode('');
    } catch (error) {
      popup.error(error?.message || (isAr ? 'فشل شحن الرصيد' : 'Failed to top up wallet'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 8 : 0}
    >
      <StatusBar barStyle="dark-content" backgroundColor="#000000" translucent={false} />

      <LinearGradient
        colors={['#0D1B4B', '#1A237E', '#4361EE']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={[styles.header, { paddingTop: Math.max(insets.top + 8, SPACING.xl) }]}
      >
        <View style={styles.headerGlowPrimary} />
        <View style={styles.headerGlowSecondary} />

        <View style={[styles.headerRow, { flexDirection: isAr ? 'row-reverse' : 'row' }]}>
          <TouchableOpacity style={styles.headerButton} onPress={() => navigation.goBack && navigation.goBack()}>
            <Ionicons name={isAr ? 'chevron-forward' : 'chevron-back'} size={22} color={COLORS.white} />
          </TouchableOpacity>

          <View style={styles.headerCenter}>
            <Text style={styles.headerTitle}>{isAr ? 'شحن الرصيد' : 'Recharge Wallet'}</Text>
            <Text style={styles.headerSubtitle}>{isAr ? 'أضف رصيداً إلى محفظتك داخل المنصة' : 'Top up your in-app wallet balance'}</Text>
          </View>

          <View style={styles.headerSpacer} />
        </View>
      </LinearGradient>

      {loading ? (
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      ) : (
        <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
          <View style={styles.balanceCard}>
            <Text style={styles.balanceLabel}>{isAr ? 'الرصيد الحالي' : 'Current Balance'}</Text>
            <Text style={styles.balanceValue}>{formatCurrency(wallet.balance, isAr)}</Text>
            <Text style={styles.balanceMeta}>
              {isAr ? `إجمالي الشحن: ${formatCurrency(wallet.totalTopups, isAr)}` : `Total top-ups: ${formatCurrency(wallet.totalTopups, isAr)}`}
            </Text>
          </View>

          <View style={styles.sectionCard}>
            <Text style={[styles.sectionTitle, { textAlign: isAr ? 'right' : 'left' }]}>
              {isAr ? 'الشحن يتم فقط بواسطة أكواد البطاقات الجاهزة' : 'Top-up works only with available recharge card codes'}
            </Text>
            <Text style={[styles.helperText, { textAlign: isAr ? 'right' : 'left' }]}>
              {isAr ? 'اختر بطاقة من القائمة أو اكتب الكود مباشرة ثم نفّذ الشحن.' : 'Choose a card below or type its code directly, then submit the top-up.'}
            </Text>
          </View>

          <View style={styles.sectionCard}>
            <Text style={[styles.sectionTitle, { textAlign: isAr ? 'right' : 'left' }]}>
              {isAr ? 'بطاقات التعبئة الجاهزة' : 'Available Recharge Cards'}
            </Text>

            {sortedCards.map((card) => {
              const active = selectedCode === card.code;
              return (
                <TouchableOpacity
                  key={card.id || card.code}
                  style={[styles.cardOption, active && styles.cardOptionActive]}
                  onPress={() => setSelectedCode(card.code)}
                  activeOpacity={0.88}
                >
                  <View style={[styles.cardOptionRow, { flexDirection: isAr ? 'row-reverse' : 'row' }]}>
                    <View style={styles.cardBadge}>
                      <Ionicons name="card-outline" size={18} color={COLORS.primaryDark} />
                    </View>
                    <View style={styles.cardTextWrap}>
                      <Text style={[styles.cardOptionTitle, { textAlign: isAr ? 'right' : 'left' }]}>
                        {isAr ? card.labelAr : card.labelEn}
                      </Text>
                      <Text style={[styles.cardOptionCode, { textAlign: isAr ? 'right' : 'left' }]}>
                        {card.code}
                      </Text>
                    </View>
                    <Text style={styles.cardOptionAmount}>{formatCurrency(card.amount, isAr)}</Text>
                  </View>
                </TouchableOpacity>
              );
            })}
          </View>

          <View style={styles.sectionCard}>
            <Text style={[styles.sectionTitle, { textAlign: isAr ? 'right' : 'left' }]}>
              {isAr ? 'إدخال كود البطاقة' : 'Enter Recharge Code'}
            </Text>

            <View style={styles.codeInputWrap}>
              <Ionicons name="key-outline" size={20} color={COLORS.textMuted} style={styles.codeInputIcon} />
              <TextInput
                value={selectedCode}
                onChangeText={setSelectedCode}
                autoCapitalize="characters"
                autoCorrect={false}
                selectionColor={COLORS.primary}
                placeholder={isAr ? 'مثال: INV-LYD-1000-0001' : 'Example: INV-LYD-1000-0001'}
                placeholderTextColor={COLORS.textMuted}
                style={[styles.codeInput, { textAlign: isAr ? 'right' : 'left', writingDirection: 'ltr' }]}
              />
            </View>

            <TouchableOpacity
              style={[styles.submitButton, (!selectedCode.trim() || submitting) && styles.submitButtonDisabled]}
              onPress={handleRedeem}
              disabled={!selectedCode.trim() || submitting}
            >
              {submitting ? (
                <ActivityIndicator size="small" color={COLORS.white} />
              ) : (
                <Text style={styles.submitButtonText}>{isAr ? 'شحن الرصيد الآن' : 'Top Up Now'}</Text>
              )}
            </TouchableOpacity>
          </View>
        </ScrollView>
      )}
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  header: {
    paddingBottom: SPACING.lg,
    paddingHorizontal: SPACING.base,
    overflow: 'hidden',
  },
  headerGlowPrimary: {
    position: 'absolute',
    top: -62,
    right: -22,
    width: 108,
    height: 108,
    borderRadius: 54,
    backgroundColor: 'rgba(255,255,255,0.14)',
  },
  headerGlowSecondary: {
    position: 'absolute',
    left: -46,
    bottom: -76,
    width: 116,
    height: 116,
    borderRadius: 58,
    backgroundColor: 'rgba(255,255,255,0.08)',
  },
  headerRow: {
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  headerButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(255,255,255,0.16)',
  },
  headerSpacer: {
    width: 40,
  },
  headerCenter: {
    flex: 1,
    alignItems: 'center',
    paddingHorizontal: SPACING.sm,
  },
  headerTitle: {
    color: COLORS.white,
    fontSize: FONTS.lg,
    fontWeight: FONTS.bold,
  },
  headerSubtitle: {
    color: 'rgba(255,255,255,0.78)',
    fontSize: FONTS.sm,
    marginTop: 2,
    textAlign: 'center',
  },
  loadingWrap: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  content: {
    padding: SPACING.base,
    gap: SPACING.base,
    paddingBottom: SPACING.xxxl,
  },
  balanceCard: {
    backgroundColor: COLORS.white,
    borderRadius: 26,
    padding: SPACING.lg,
    borderWidth: 1,
    borderColor: COLORS.borderLight,
    ...SHADOWS.md,
  },
  balanceLabel: {
    fontSize: FONTS.sm,
    color: COLORS.textSecondary,
    marginBottom: SPACING.xs,
  },
  balanceValue: {
    fontSize: 34,
    color: COLORS.teal,
    fontWeight: FONTS.bold,
  },
  balanceMeta: {
    marginTop: SPACING.xs,
    fontSize: FONTS.sm,
    color: COLORS.primary,
    fontWeight: FONTS.semibold,
  },
  sectionCard: {
    backgroundColor: COLORS.white,
    borderRadius: 24,
    padding: SPACING.base,
    borderWidth: 1,
    borderColor: COLORS.borderLight,
    ...SHADOWS.sm,
  },
  sectionTitle: {
    fontSize: FONTS.base,
    color: COLORS.textPrimary,
    fontWeight: FONTS.bold,
    marginBottom: SPACING.sm,
  },
  helperText: {
    fontSize: FONTS.sm,
    color: COLORS.textMuted,
    lineHeight: 21,
  },
  cardOption: {
    borderRadius: 18,
    borderWidth: 1,
    borderColor: COLORS.border,
    backgroundColor: COLORS.background,
    padding: SPACING.md,
    marginBottom: SPACING.sm,
  },
  cardOptionActive: {
    borderColor: COLORS.primary,
    backgroundColor: COLORS.primaryLight,
  },
  cardOptionRow: {
    alignItems: 'center',
    gap: SPACING.sm,
  },
  cardBadge: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: COLORS.white,
    alignItems: 'center',
    justifyContent: 'center',
  },
  cardTextWrap: {
    flex: 1,
  },
  cardOptionTitle: {
    fontSize: FONTS.sm,
    color: COLORS.textPrimary,
    fontWeight: FONTS.bold,
  },
  cardOptionCode: {
    marginTop: 2,
    fontSize: FONTS.xs + 1,
    color: COLORS.textMuted,
  },
  cardOptionAmount: {
    color: COLORS.primaryDark,
    fontWeight: FONTS.bold,
    fontSize: FONTS.sm,
  },
  codeInput: {
    minHeight: 56,
    backgroundColor: COLORS.white,
    paddingHorizontal: SPACING.sm,
    paddingVertical: SPACING.md,
    color: COLORS.textPrimary,
    fontSize: FONTS.base,
    fontWeight: FONTS.semibold,
    flex: 1,
  },
  codeInputWrap: {
    minHeight: 58,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: COLORS.border,
    backgroundColor: COLORS.white,
    paddingHorizontal: SPACING.base,
    flexDirection: 'row',
    alignItems: 'center',
    ...SHADOWS.sm,
  },
  codeInputIcon: {
    marginRight: SPACING.xs,
  },
  submitButton: {
    marginTop: SPACING.md,
    borderRadius: RADIUS.full,
    backgroundColor: COLORS.primaryDark,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: SPACING.md,
    ...SHADOWS.button,
  },
  submitButtonDisabled: {
    opacity: 0.45,
  },
  submitButtonText: {
    color: COLORS.white,
    fontSize: FONTS.base,
    fontWeight: FONTS.bold,
  },
});

export default RechargeWalletScreen;
