<template>
  <div class="wishlist-page min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <!-- NavBar Component -->
    <NavBar/>

    <!-- Main Content -->
    <main class="flex-1 container mx-auto px-4 py-6">
      <h1 class="text-3xl font-bold text-[#F5DEB3] mb-6">Your Wishlist</h1>

      <!-- Empty state -->
      <div v-if="!wishlistItems.length" class="flex flex-col items-center justify-center text-[#aaa] py-20">
        <p class="text-lg mb-4">Your wishlist is currently empty.</p>
        <router-link
            to="/shop"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
        >
          Browse Books
        </router-link>
      </div>

      <!-- Wishlist items -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <transition-group name="fade" tag="div" class="contents">
          <div
              v-for="item in wishlistItems"
              :key="item.id"
              class="bg-[#1a1a1a] rounded-xl p-5 flex flex-col shadow border border-[#333] hover:shadow-md transition"
          >
            <div class="relative">
              <img
                  :src="normalizeImage(item.images[0])"
                  :alt="item.title"
                  class="w-full h-48 object-cover rounded mb-4"
              />
              <button
                  @click="removeFromWishlist(item)"
                  class="absolute top-2 right-2 w-8 h-8 bg-[#333] bg-opacity-70 rounded-full flex items-center justify-center hover:bg-red-600 transition"
              >
                <i class="fas fa-times text-white"></i>
              </button>
            </div>

            <div class="flex-1">
              <h2 class="text-xl font-semibold truncate text-[#F5DEB3]">{{ item.title }}</h2>
              <p class="text-sm text-[#aaa] line-clamp-2 my-2">{{ item.description }}</p>
              <p class="text-sm text-[#C8B280] mb-4">
                Price: <span class="font-bold">${{ item.price.toFixed(2) }}</span>
              </p>
            </div>

            <div class="mt-auto flex justify-between items-center">
              <button
                  @click="addToCart(item)"
                  class="px-4 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition flex-1 mr-2"
              >
                <i class="fas fa-shopping-cart mr-2"></i>Add to Cart
              </button>
              <button
                  @click="shareItem(item)"
                  class="w-10 h-10 bg-[#333] rounded flex items-center justify-center hover:bg-[#444] transition"
              >
                <i class="fas fa-share-alt text-[#C8B280]"></i>
              </button>
            </div>
          </div>
        </transition-group>
      </div>

      <!-- Action buttons -->
      <div
          v-if="wishlistItems.length"
          class="mt-8 flex flex-col md:flex-row justify-center space-y-4 md:space-y-0 md:space-x-4"
      >
        <button
            @click="addAllToCart"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
        >
          Add All to Cart
        </button>
        <button
            @click="clearWishlist"
            class="px-6 py-2 bg-[#8B0000] text-white rounded hover:bg-[#a30000] transition"
        >
          Clear Wishlist
        </button>
      </div>
    </main>

    <!-- Share Modal -->
    <div v-if="showShareModal" class="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-[#1a1a1a] p-6 rounded-xl w-full max-w-md">
        <h3 class="text-xl font-bold text-[#F5DEB3] mb-4">Share This Book</h3>

        <div class="flex flex-col space-y-4">
          <div class="flex items-center space-x-2">
            <input
                type="text"
                :value="shareUrl"
                readonly
                class="flex-1 bg-[#333] border border-[#444] text-[#C8B280] px-3 py-2 rounded"
            />
            <button
                @click="copyToClipboard"
                class="px-3 py-2 bg-[#444] rounded hover:bg-[#555]"
            >
              <i class="fas fa-copy"></i>
            </button>
          </div>

          <div class="flex justify-center space-x-6 mt-2">
            <a href="#" @click.prevent="shareOnSocial('facebook')" class="text-2xl text-[#3b5998] hover:opacity-80">
              <i class="fab fa-facebook"></i>
            </a>
            <a href="#" @click.prevent="shareOnSocial('twitter')" class="text-2xl text-[#1da1f2] hover:opacity-80">
              <i class="fab fa-twitter"></i>
            </a>
            <a href="#" @click.prevent="shareOnSocial('whatsapp')" class="text-2xl text-[#25d366] hover:opacity-80">
              <i class="fab fa-whatsapp"></i>
            </a>
          </div>
        </div>

        <button
            @click="showShareModal = false"
            class="mt-6 w-full px-4 py-2 bg-[#333] text-[#C8B280] rounded hover:bg-[#444] transition"
        >
          Close
        </button>
      </div>
    </div>

    <!-- Footer Component -->
    <FooterComponent/>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import FooterComponent from '@/components/Footer.vue';

// Base URL for your API (images live here)
const baseUrl = process.env.VUE_APP_API_URL || window.location.origin;

// UI & Auth State
const isLoggedIn     = ref(false);
const showShareModal = ref(false);
const currentShareItem = ref(null);
const shareUrl       = ref('');

// Wishlist Data
const wishlistItems = ref([]);

// Toasts
const toasts = reactive([]);
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => {
    const idx = toasts.findIndex(t => t.id === id);
    if (idx > -1) toasts.splice(idx, 1);
  }, 5000);
}

// Auth header
function authHeader() {
  const token = localStorage.getItem('authToken');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

// Normalize an image path to an absolute URL
function normalizeImage(path) {
  if (!path) return '/fallback.png';
  if (/^https?:\/\//.test(path)) {
    return path;
  }
  // Prepend your API base URL
  return `${baseUrl.replace(/\/$/, '')}${path.startsWith('/') ? '' : '/'}${path}`;
}

// Fetch the user's wishlist
async function fetchWishlist() {
  if (!localStorage.getItem('authToken')) return;
  try {
    const res = await axios.get(`${baseUrl}/api/bookmarks`, {
      headers: authHeader()
    });
    wishlistItems.value = res.data.data.map(b => ({
      id: b.bookId,
      title: b.title,
      description: b.description,
      price: b.price,
      images: b.images
    }));
  } catch {
    showToast('Failed to load wishlist', 'error');
  }
}

// Remove a single item
async function removeFromWishlist(item) {
  try {
    await axios.delete(`${baseUrl}/api/bookmarks/${item.id}`, {
      headers: authHeader()
    });
    wishlistItems.value = wishlistItems.value.filter(i => i.id !== item.id);
    showToast('Removed from wishlist');
  } catch {
    showToast('Failed to remove', 'error');
  }
}

// Clear entire wishlist
async function clearWishlist() {
  try {
    await Promise.all(
        wishlistItems.value.map(i =>
            axios.delete(`${baseUrl}/api/bookmarks/${i.id}`, {
              headers: authHeader()
            })
        )
    );
    wishlistItems.value = [];
    showToast('Wishlist cleared');
  } catch {
    showToast('Failed to clear', 'error');
  }
}

// Add all to cart (stub)
function addToCart(item) {
  showToast('Added to cart');
}

// Add all items
function addAllToCart() {
  wishlistItems.value.forEach(i => addToCart(i));
  showToast('All added to cart');
}

// Share flow
function shareItem(item) {
  currentShareItem.value = item;
  shareUrl.value = `${window.location.origin}/shop/book/${item.id}`;
  showShareModal.value = true;
}

function copyToClipboard() {
  navigator.clipboard.writeText(shareUrl.value)
      .then(() => showToast('Copied'))
      .catch(() => showToast('Copy failed','error'));
}

function shareOnSocial(platform) {
  const text = `Check out ${currentShareItem.value.title}`;
  let url = '';
  switch(platform) {
    case 'facebook': url = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl.value)}`; break;
    case 'twitter':  url = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(shareUrl.value)}`; break;
    case 'whatsapp': url = `https://wa.me/?text=${encodeURIComponent(text + ' ' + shareUrl.value)}`; break;
  }
  window.open(url, '_blank');
  showShareModal.value = false;
}

// Initialization
onMounted(() => {
  isLoggedIn.value = !!localStorage.getItem('authToken');
  if (isLoggedIn.value) fetchWishlist();
});
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.fade-enter-active, .fade-leave-active {
  transition: all 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
.contents {
  display: contents;
}
</style>
