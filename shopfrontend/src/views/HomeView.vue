<template>
  <div class="home-container min-h-screen bg-gray-900 text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Hero Section -->
    <section class="bg-[#1a1a1a] text-center py-20">
      <h1 class="text-4xl sm:text-5xl font-bold mb-4">Discover Your Next Favorite Book</h1>
      <p class="text-lg sm:text-xl text-[#aaa] mb-8">From new arrivals to top-rated exclusives.</p>
      <router-link
          to="/shop"
          class="bg-[#A0522D] hover:bg-[#8B4513] text-white px-6 py-3 rounded transition"
      >
        Browse All Books
      </router-link>
    </section>

    <!-- Dynamic Book Sections -->
    <section
        v-for="(section, index) in bookSections"
        :key="index"
        class="py-16 px-4 sm:px-8 bg-gray-900 border-t border-[#333]"
    >
      <div class="container mx-auto">
        <h2 class="text-3xl font-semibold mb-8 text-center">{{ section.title }}</h2>
        <div v-if="section.loading" class="flex justify-center py-10">
          <div class="loader h-12 w-12 border-t-4 border-[#C8B280] rounded-full animate-spin"></div>
        </div>
        <div v-else-if="section.books.length === 0" class="text-center text-[#888] text-lg">No books found.</div>
        <div
            v-else
            class="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
        >
          <ProductCard
              v-for="book in section.books"
              :key="book.id"
              :book="book"
              @showBookDetail="openProductDetail"
          >
            <template #action-button>
              <AddToCartButton :product="book" @addToCart="handleAddToCart" />
              <button
                  @click="addToWishlist(book)"
                  class="ml-2 p-2 bg-[#2a2a2a] rounded hover:bg-[#3a3a3a] transition"
                  title="Add to Wishlist"
              >
                <font-awesome-icon :icon="['fas','heart']" class="text-lg text-[#C8B280]" />
              </button>
            </template>
          </ProductCard>
        </div>
      </div>
    </section>

    <!-- Product Detail Modal -->
    <ProductDetail v-if="showProductDetail" :book="selectedBook" @close="closeProductDetail">
      <template #flag>
        <div class="ml-4 mt-1">
          <FlagButton v-if="isAuthenticated" type="product" :id="selectedBook.id" />
        </div>
      </template>
      <template #add-to-cart>
        <AddToCartButton :product="selectedBook" @addToCart="handleAddToCart" />
      </template>
    </ProductDetail>

    <Footer />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import Toast from '@/components/Toast.vue';
import ProductCard from '@/components/ProductCard.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import AddToCartButton from '@/components/AddToCartButton.vue';
import FlagButton from '@/components/FlagButton.vue';
import { useCart } from '@/store/useCart';

const { addToCart } = useCart();
const apiBaseUrl = process.env.VUE_APP_API_URL;
const token = () => localStorage.getItem('authToken');
const isAuthenticated = !!token();

// Toasts
const toasts = reactive([]);
const showToast = (message, type = 'success') => {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => toasts.splice(toasts.findIndex(t => t.id === id), 1), 5000);
};

// Detail modal
const showProductDetail = ref(false);
const selectedBook = ref(null);
const openProductDetail = (book) => {
  selectedBook.value = book;
  showProductDetail.value = true;
  document.body.style.overflow = 'hidden';
};
const closeProductDetail = () => {
  selectedBook.value = null;
  showProductDetail.value = false;
  document.body.style.overflow = '';
};

// Book sections
const bookSections = ref([
  { title: 'On Discount', route: 'discounted', books: [], loading: true },
  { title: 'Top Rated', route: 'top-rated', books: [], loading: true },
  { title: 'Best Sellers', route: 'best-sellers', books: [], loading: true },
  { title: 'New Arrivals', route: 'new-arrivals', books: [], loading: true },
  { title: 'Exclusive Editions', route: 'exclusive', books: [], loading: true }
]);

// Fetch books from each section
async function loadSectionBooks() {
  for (const section of bookSections.value) {
    section.loading = true;
    try {
      const res = await axios.get(`${apiBaseUrl}/api/home/${section.route}`);
      section.books = (res.data || []).map(book => ({
        ...book,
        category: { name: book.genre?.name || '' },
        images: book.images?.length
            ? book.images.map(img => `${apiBaseUrl}${img}`)
            : [`${apiBaseUrl}/images/default-book.jpg`],
      }));
    } catch {
      section.books = [];
    } finally {
      section.loading = false;
    }
  }
}


// Cart/Wishlist
const handleAddToCart = async ({ product, quantity }) => {
  const res = await addToCart(product, quantity);
  showToast(res.message, res.success ? 'success' : 'error');
};
const addToWishlist = async (book) => {
  if (!isAuthenticated) {
    showToast('Please log in to add to wishlist', 'error');
    return;
  }
  try {
    const res = await axios.post(
        `${apiBaseUrl}/api/bookmarks/${book.id}`,
        {},
        { headers: { Authorization: `Bearer ${token()}` } }
    );
    showToast(res.data.message || 'Added to wishlist');
  } catch (err) {
    showToast(err.response?.data?.message || 'Failed to add to wishlist', 'error');
  }
};

onMounted(() => {
  loadSectionBooks();
});
</script>

<style scoped>
.loader {
  border-top-color: #A0522D;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
