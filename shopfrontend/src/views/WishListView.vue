<template>
  <div class="wishlist-page min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-1 container mx-auto px-4 py-6">
      <h1 class="text-3xl font-bold text-[#F5DEB3] mb-6">Your Wishlist</h1>

      <!-- Empty state -->
      <div v-if="!wishlistItems.length" class="text-center py-20 text-[#aaa]">
        <p class="text-lg mb-4">Your wishlist is currently empty.</p>
        <router-link to="/shop" class="bg-[#A0522D] text-white px-6 py-2 rounded hover:bg-[#8B4513] transition">
          Browse Books
        </router-link>
      </div>

      <!-- Wishlist grid -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <transition-group name="fade" tag="div" class="contents">
          <div
              v-for="item in wishlistItems"
              :key="item.id"
              class="bg-[#1a1a1a] border border-[#333] rounded-xl p-4 shadow hover:shadow-lg transition relative flex flex-col"
          >
            <!-- Remove Button -->
            <button
                @click="removeFromWishlist(item)"
                class="absolute top-2 right-2 bg-[#2a2a2a] hover:bg-red-600 text-white rounded-full w-8 h-8 flex items-center justify-center transition"
                title="Remove from wishlist"
            >
              X
            </button>

            <!-- Book Image -->
            <img
                :src="normalizeImage(item.images[0])"
                :alt="item.title"
                class="w-full h-48 object-cover rounded mb-4"
            />

            <!-- Book Info -->
            <h2 class="text-xl font-semibold truncate text-[#F5DEB3]">{{ item.title }}</h2>
            <p class="text-sm text-[#aaa] my-2 line-clamp-2">{{ item.description }}</p>
            <p class="text-sm text-[#C8B280] mb-4">
              Price: <span class="font-bold">${{ item.price.toFixed(2) }}</span>
            </p>

            <!-- Action Buttons -->
            <div class="mt-auto flex items-center space-x-2">
              <AddToCartButton :product="item" @addToCart="handleAddToCart" />
             
            </div>
          </div>
        </transition-group>
      </div>

      <!-- Bulk Action Buttons -->
      <div v-if="wishlistItems.length" class="mt-8 flex flex-col md:flex-row justify-center space-y-4 md:space-y-0 md:space-x-4">
        <button
            @click="addAllToCart"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
            :disabled="loadingCart"
        >
          {{ loadingCart ? 'Processing...' : 'Add All to Cart' }}
        </button>
        <button
            @click="clearWishlist"
            class="px-6 py-2 bg-[#8B0000] text-white rounded hover:bg-[#a30000] transition"
        >
          Clear Wishlist
        </button>
      </div>
    </main>

    <FooterComponent />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import axios from 'axios'

import NavBar from '@/components/NavBar.vue'
import FooterComponent from '@/components/Footer.vue'
import Toast from '@/components/Toast.vue'
import AddToCartButton from '@/components/AddToCartButton.vue'
import { useCart } from '@/store/useCart'

const { addToCart } = useCart()

const baseUrl = process.env.VUE_APP_API_URL || window.location.origin
const wishlistItems = ref([])
const loadingCart = ref(false)
const toasts = reactive([])

// Toast utils
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 5000)
}
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id)
  if (i !== -1) toasts.splice(i, 1)
}

// Helpers
function normalizeImage(path) {
  if (!path) return '/fallback.png'
  if (/^https?:\/\//.test(path)) return path
  return `${baseUrl.replace(/\/$/, '')}${path.startsWith('/') ? '' : '/'}${path}`
}
function authHeader() {
  const token = localStorage.getItem('authToken')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

// Load Wishlist
async function fetchWishlist() {
  try {
    const res = await axios.get(`${baseUrl}/api/bookmarks`, {
      headers: authHeader()
    })
    wishlistItems.value = res.data.data.map(b => ({
      id: b.bookId,
      title: b.title,
      description: b.description,
      price: b.price,
      images: b.images
    }))
  } catch {
    showToast('Failed to load wishlist', 'error')
  }
}

// Add to cart
async function handleAddToCart({ product, quantity }) {
  loadingCart.value = true
  const result = await addToCart(product, quantity)
  showToast(result.message, result.success ? 'success' : 'error')
  loadingCart.value = false
}

// Add all to cart
async function addAllToCart() {
  loadingCart.value = true
  const failed = []
  for (const item of wishlistItems.value) {
    const result = await addToCart(item, 1)
    if (!result.success) failed.push(item.title)
  }
  showToast(
      failed.length ? `Failed: ${failed.join(', ')}` : 'All added to cart',
      failed.length ? 'error' : 'success'
  )
  loadingCart.value = false
}

// Remove item
async function removeFromWishlist(item) {
  try {
    await axios.delete(`${baseUrl}/api/bookmarks/${item.id}`, {
      headers: authHeader()
    })
    wishlistItems.value = wishlistItems.value.filter(i => i.id !== item.id)
    showToast('Removed from wishlist')
  } catch {
    showToast('Failed to remove item', 'error')
  }
}

// Clear all
async function clearWishlist() {
  try {
    await Promise.all(
        wishlistItems.value.map(item =>
            axios.delete(`${baseUrl}/api/bookmarks/${item.id}`, {
              headers: authHeader()
            })
        )
    )
    wishlistItems.value = []
    showToast('Wishlist cleared')
  } catch {
    showToast('Failed to clear wishlist', 'error')
  }
}

// Share
function shareItem(item) {
  const url = `${window.location.origin}/shop/book/${item.id}`
  navigator.clipboard.writeText(url)
      .then(() => showToast('Link copied'))
      .catch(() => showToast('Copy failed', 'error'))
}

onMounted(fetchWishlist)
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
