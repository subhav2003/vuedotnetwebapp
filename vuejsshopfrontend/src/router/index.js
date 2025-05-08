import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import TestView from '@/views/TestView.vue';
import ShopView from '@/views/ShopView.vue';
import ConnectView from '@/views/ConnectView.vue';
import CartView from '@/views/CartView.vue';
import CheckoutView from '@/views/CheckoutView.vue';
import PaymentSuccessView from '@/views/PaymentSuccessView.vue';
import CustomOrderPaySuccessView from '@/views/CustomOrderPaySuccessView.vue';
import ProfileView from '@/views/ProfileView.vue';
import AuthView from '@/views/AuthView.vue';
import PasswordResetView from '@/views/PasswordResetView.vue';
import ArtisanView from '@/views/Artisans/ArtisanView.vue';
import ProductManagementView from '@/views/Artisans/ProductManagementView.vue';
import ProfileManagementView from '@/views/Artisans/ProfileManagementView.vue';
import OrderManagementView from '@/views/Artisans/OrderManagementView.vue';
import AdminView from '@/views/Admin/AdminView.vue';
import WhistlistView from "@/views/WhistlistView.vue";

const PUBLIC_NAMES = ['home', 'shop', 'connect', 'passwordreset', 'auth'];

const routes = [
  { path: '/test', name: 'test', component: TestView },
  { path: '/', name: 'home', component: HomeView },
  { path: '/shop', name: 'shop', component: ShopView },
  { path: '/connect', name: 'connect', component: ConnectView },
  { path: '/passwordreset', name: 'passwordreset', component: PasswordResetView },
  { path: '/auth', name: 'auth', component: AuthView },
  { path: '/cart', name: 'cart', component: CartView },
  { path: '/checkout', name: 'checkout', component: CheckoutView },
  { path: '/payment/success', name: 'paymentsuccess', component: PaymentSuccessView },
  { path: '/custom/payment/success', name: 'custompaymentsuccess', component: CustomOrderPaySuccessView },
  { path: '/profile', name: 'profile', component: ProfileView },
  { path: '/whistlist', name: 'whistlis', component: WhistlistView },

  // Admin routes (includes artisan pages)
  { path: '/artisan', name: 'artisan', component: ArtisanView, meta: { requiresAdmin: true } },
  { path: '/artisan/productmanagement', name: 'artisanproductmanagement', component: ProductManagementView, meta: { requiresAdmin: true } },
  { path: '/artisan/profilemanagement', name: 'artisanprofilemanagement', component: ProfileManagementView, meta: { requiresAdmin: true } },
  { path: '/artisan/ordermanagement', name: 'artisanordermanagement', component: OrderManagementView, meta: { requiresAdmin: true } },
  { path: '/admin', name: 'admin', component: AdminView, meta: { requiresAdmin: true } }
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('authToken');
  const role = localStorage.getItem('userRole'); // now 'member' or 'admin'

  // 1) Not logged in → allow only public pages
  if (!token) {
    return PUBLIC_NAMES.includes(to.name) ? next() : next({ name: 'auth' });
  }

  // 2) Prevent authenticated users from visiting /auth
  if (to.name === 'auth') {
    return next({ name: 'home' });
  }

  // 3) Admin-only routes
  if (to.meta.requiresAdmin && role !== 'admin') {
    return next({ name: 'home' });
  }

  // 4) Member-only pages (e.g. profile)
  if (role === 'member' && to.meta.requiresAdmin) {
    return next({ name: 'home' });
  }

  next(); // allow access
});

export default router;
