<template>
  <div class="home-view flex flex-col min-h-screen bg-gray-50">
    <!-- Navbar -->
    <NavBar />

    <!-- Toast stack (teleported to body) -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Hero Section -->
    <section class="bg-gradient-to-r from-yellow-100 via-yellow-50 to-yellow-100 py-20 text-center">
      <h1 class="text-5xl sm:text-6xl font-black text-brown-800 mb-4 font-papyrus">
        Welcome to CraftConnect
      </h1>
      <p class="text-xl sm:text-2xl text-brown-600 mb-8">
        Discover products and artisans loved by users—shop best-sellers or connect directly.
      </p>
      <button
          @click="openShop"
          class="inline-block bg-brown-600 hover:bg-brown-700 text-white font-semibold py-3 px-8 rounded-lg shadow-lg transition-transform transform hover:-translate-y-1"
      >
        Explore Marketplace
      </button>
    </section>

    <!-- Products Section -->
    <section class="py-16 w-full">
      <div class="flex flex-col md:flex-row w-full">
        <!-- Left Panel -->
        <div class="bg-brown-600 text-white p-6 flex-shrink-0 md:w-1/3">
          <h2 class="text-3xl font-bold mb-2 font-papyrus">Discover Products Loved by Our Community</h2>
          <p>Each item you see here is hand‑selected from top‑selling treasures crafted by our artisans. From timeless keepsakes to everyday essentials, our community can’t get enough of these favorites—curated for authenticity, quality, and a personal touch.</p>
        </div>
        <!-- Right Panel -->
        <div class="relative bg-white border border-gray-200 p-6 flex-1 overflow-hidden">
          <div
              ref="productScroll"
              @scroll.passive="onScroll('product')"
              class="flex space-x-6 overflow-x-auto pb-4 scroll-smooth"
          >
            <template v-if="loadingProducts">
              <div
                  v-for="n in 5"
                  :key="n"
                  class="w-60 h-80 bg-gray-100 shadow animate-pulse flex-shrink-0"
              ></div>
            </template>
            <template v-else>
              <ProductCard
                  v-for="product in topProducts"
                  :key="product.id"
                  :product="product"
                  @showProductDetail="openProductDetail"
                  class="flex-shrink-0"
              >
                <template #flag>
                  <FlagButton v-if="isAuthenticated" type="product" :id="product.id" />
                </template>
                <template #action-button>
                  <AddToCartButton
                      :product="product"
                      @addToCart="handleAddToCart"
                  />
                </template>
              </ProductCard>
            </template>
          </div>
        </div>
      </div>
    </section>

    <!-- Artisans Section -->
    <section class="py-16 w-full bg-white">
      <div class="flex flex-col md:flex-row w-full">
        <!-- Left Panel -->
        <div class="bg-brown-600 text-white p-6 flex-shrink-0 md:w-1/3">
          <h2 class="text-3xl font-bold mb-2 font-papyrus">Discover Artisans Loved by Our Community</h2>
          <p>These skilled creators have earned top ratings for their passion, craftsmanship, and attention to detail. From seasoned masters to rising talents, our community trusts these artisans to bring your custom ideas to life and deliver one‑of‑a‑kind treasures you’ll cherish.</p>
        </div>
        <!-- Right Panel -->
        <div class="relative bg-white border border-gray-200 p-6 flex-1 overflow-hidden">
          <div
              ref="artisanScroll"
              @scroll.passive="onScroll('artisan')"
              class="flex space-x-6 overflow-x-auto pb-4 scroll-smooth"
          >
            <template v-if="loadingArtisans">
              <div
                  v-for="n in 5"
                  :key="n"
                  class="w-60 h-80 bg-gray-100 shadow animate-pulse flex-shrink-0"
              ></div>
            </template>
            <template v-else>
              <ArtisanCard
                  v-for="artisan in topArtisans"
                  :key="artisan.id"
                  :artisan="artisan"
                  @viewProfile="openPortfolio"
                  class="flex-shrink-0"
              >
                <template #flag>
                  <FlagButton v-if="isAuthenticated" type="artisan" :id="artisan.id" />
                </template>
              </ArtisanCard>
            </template>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section class="py-16 bg-beige text-center">
      <h2 class="text-3xl sm:text-4xl font-bold text-brown-800 mb-6 font-papyrus">
        About CraftConnect
      </h2>
      <p class="text-lg text-brown-600 mb-8 max-w-2xl mx-auto">
        CraftConnect is an artisan-driven marketplace connecting creators with people who treasure handmade craftsmanship. We ensure authenticity, transparency, and a personal touch in every item.
      </p>
      <button
          @click="openBecomeArtisan"
          class="inline-block bg-brown-600 hover:bg-brown-700 text-white font-semibold py-3 px-8 rounded-lg shadow-lg transition-transform transform hover:-translate-y-1"
      >
        Explore Artisans
      </button>
    </section>

    <!-- Footer -->
    <Footer />

    <!-- Product Detail Modal -->
    <ProductDetail
        v-if="showProductDetail"
        :product="selectedProduct"
        @close="closeProductDetail"
    >
      <template #flag>
        <FlagButton v-if="isAuthenticated" type="product" :id="selectedProduct.id" />
      </template>
      <template #add-to-cart>
        <AddToCartButton :product="selectedProduct" @addToCart="handleAddToCart" />
      </template>
    </ProductDetail>

    <!-- Portfolio Modal -->
    <Portfolio
        v-if="showPortfolio"
        :artisanId="selectedArtisanId"
        @close="closePortfolio"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import ProductCard from '@/components/ProductCard.vue';
import ArtisanCard from '@/components/ArtisanCard.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import Portfolio from '@/components/Portfolio.vue';
import AddToCartButton from '@/components/AddToCartButton.vue';
import FlagButton from '@/components/FlagButton.vue';
import Toast from '@/components/Toast.vue';
import { useCart } from '@/store/useCart';
import { useRouter } from 'vue-router';

const { addToCart } = useCart();
const router = useRouter();
const apiBase = process.env.VUE_APP_API_URL;
const isAuthenticated = !!localStorage.getItem('authToken');

// Data
const topProducts = ref([]);
const topArtisans = ref([]);
const loadingProducts = ref(true);
const loadingArtisans = ref(true);

// Scroll state
const productScroll = ref(null);
const artisanScroll = ref(null);

// Detail & portfolio
const showProductDetail = ref(false);
const selectedProduct = ref(null);
const showPortfolio = ref(false);
const selectedArtisanId = ref(null);

// Toast stack
const toasts = reactive([]);
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id);
  if (i !== -1) toasts.splice(i, 1);
}
function showToast(msg, type='success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message: msg, type });
  setTimeout(() => removeToast(id), 5000);
}

// Fetch top-sellers
async function fetchTopProducts() {
  loadingProducts.value = true;
  try {
    const res = await axios.get(`${apiBase}/api/v1/products/top-selling?limit=10`);
    topProducts.value = res.data.data;
    setTimeout(() => onScroll('product'), 0);
  } catch (e) {
    showToast('Failed to load products', 'error');
  } finally {
    loadingProducts.value = false;
  }
}

// Fetch top-rated artisans
async function fetchTopArtisans() {
  loadingArtisans.value = true;
  try {
    const res = await axios.get(`${apiBase}/api/v1/artisans/top-rated?limit=10`);
    topArtisans.value = res.data.data;
    setTimeout(() => onScroll('artisan'), 0);
  } catch {
    showToast('Failed to load artisans', 'error');
  } finally {
    loadingArtisans.value = false;
  }
}

// Scrolling
function onScroll(type) {
  const el = type==='product' ? productScroll.value : artisanScroll.value;
  if (!el) return;
  const max = el.scrollWidth - el.clientWidth;
  // no need to track arrows now
}

// Actions
function openShop() {
  router.push({ name: 'shop' });
}
function openBecomeArtisan() {
  router.push({ name: 'connect' });
}
function openProductDetail(prod) {
  selectedProduct.value = prod;
  showProductDetail.value = true;
  document.body.style.overflow = 'hidden';
}
function closeProductDetail() {
  showProductDetail.value = false;
  document.body.style.overflow = '';
}
function openPortfolio(id) {
  selectedArtisanId.value = id;
  showPortfolio.value = true;
  document.body.style.overflow = 'hidden';
}
function closePortfolio() {
  showPortfolio.value = false;
  document.body.style.overflow = '';
}

// Cart
async function handleAddToCart({ product, quantity }) {
  const res = await addToCart(product, quantity);
  showToast(res.message, res.success ? 'success' : 'error');
}

onMounted(() => {
  fetchTopProducts();
  fetchTopArtisans();
  window.addEventListener('resize', () => onScroll('product') || onScroll('artisan'));
});
</script>

<style scoped>
@media (min-width: 768px) {
  [ref="productScroll"], [ref="artisanScroll"] {
    overflow-x: auto;
  }
}
@media (max-width: 767px) {
  [ref="productScroll"]::-webkit-scrollbar,
  [ref="artisanScroll"]::-webkit-scrollbar {
    display: none;
  }
}
.bg-beige { background-color: #F5F5DC; }
.font-papyrus { font-family: 'Papyrus', serif; }
/* Remove pointer cursor on links/buttons */
a, button { cursor: default !important; }
</style>
