<template>
  <div class="app bg-gray-900 text-gray-200 min-h-screen">
    <!-- Header section -->
    <NavBar />

    <!-- Hero section -->
    <section class="hero bg-gray-800 py-20">
      <div class="max-w-6xl mx-auto px-4 text-center">
        <h1 class="text-4xl md:text-5xl font-bold mb-4">Discover your next favorite book</h1>
        <p class="text-lg md:text-xl text-gray-400 mb-8">Explore our collection of handpicked books from various genres.</p>
        <router-link
            to="/shop"
            class="inline-block bg-[#b25e31] hover:bg-[#a04a1a] text-white px-6 py-3 rounded transition"
        >
          Browse Books
        </router-link>
      </div>
    </section>

    <!-- Featured Books section -->
    <section class="featured-books bg-gray-900 py-20">
      <div class="max-w-6xl mx-auto px-4">
        <h2 class="text-3xl font-semibold text-center mb-12">Featured Books</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-8">
          <div
              v-for="book in featuredBooks"
              :key="book.id"
              class="book-card bg-gray-800 rounded-lg overflow-hidden hover:shadow-lg transform hover:-translate-y-1 transition"
          >
            <div class="book-img h-72 overflow-hidden">
              <img :src="book.image" :alt="book.title" class="w-full h-full object-cover" />
            </div>
            <div class="book-info p-6">
              <h3 class="text-lg font-semibold mb-2">{{ book.title }}</h3>
              <p class="text-gray-400 mb-2">{{ book.author }}</p>
              <p class="font-bold text-[#b25e31] mb-4">Rs. {{ book.price }}</p>
              <div class="flex justify-between">
                <button
                    @click="addToCart(book.id)"
                    class="bg-gray-800 hover:bg-gray-700 text-white px-4 py-2 rounded transition"
                >
                  Add to Cart
                </button>
                <button
                    @click="addToWishlist(book.id)"
                    class="border border-[#b25e31] text-[#b25e31] hover:bg-[#b25e31] hover:text-white w-9 h-9 rounded-full flex items-center justify-center transition"
                >
                  <i class="fas fa-heart"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Categories section -->
    <section class="categories bg-gray-800 py-20">
      <div class="max-w-6xl mx-auto px-4">
        <h2 class="text-3xl font-semibold text-center mb-12">Browse by Category</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-6">
          <div
              v-for="category in categories"
              :key="category.id"
              class="category-card text-center hover:shadow-lg transform hover:-translate-y-1 transition"
          >
            <router-link :to="`/category/${category.slug}`">
              <div class="category-img h-48 w-full overflow-hidden rounded-lg mb-4">
                <img :src="category.image" :alt="category.name" class="w-full h-full object-cover" />
              </div>
              <h3 class="text-lg font-medium">{{ category.name }}</h3>
            </router-link>
          </div>
        </div>
      </div>
    </section>

    <!-- Newsletter section -->
    <section class="newsletter bg-gray-900 py-20">
      <div class="max-w-md mx-auto px-4 text-center">
        <h2 class="text-3xl font-semibold mb-4">Stay Updated</h2>
        <p class="text-gray-400 mb-6">Subscribe to our newsletter and be the first to know about new releases and special offers.</p>
        <form @submit.prevent="subscribeNewsletter" class="flex flex-col sm:flex-row gap-4">
          <input
              type="email"
              v-model="newsletter.email"
              placeholder="Your email address"
              required
              class="flex-1 px-4 py-3 bg-gray-800 border border-gray-700 rounded focus:outline-none focus:border-[#b25e31] placeholder-gray-400 text-gray-200"
          />
          <button
              type="submit"
              class="bg-[#b25e31] hover:bg-[#a04a1a] text-white px-6 py-3 rounded transition"
          >
            Subscribe
          </button>
        </form>
      </div>
    </section>

    <!-- Footer section -->
    <footer class="footer bg-[#0a0a0a] py-12">
      <div class="max-w-6xl mx-auto px-4 grid grid-cols-1 md:grid-cols-3 gap-8">
        <div class="footer-about">
          <h3 class="text-xl text-white mb-4">About Pustakalaya</h3>
          <p class="text-gray-400">Pustakalaya is your go-to online bookstore for quality books across various genres.</p>
        </div>
        <div class="footer-links">
          <h3 class="text-xl text-white mb-4">Quick Links</h3>
          <ul class="space-y-2">
            <li><router-link to="/about" class="hover:text-[#b25e31]">About Us</router-link></li>
            <li><router-link to="/contact" class="hover:text-[#b25e31]">Contact Us</router-link></li>
            <li><router-link to="/terms" class="hover:text-[#b25e31]">Terms of Service</router-link></li>
            <li><router-link to="/privacy" class="hover:text-[#b25e31]">Privacy Policy</router-link></li>
          </ul>
        </div>
        <div class="footer-social">
          <h3 class="text-xl text-white mb-4">Follow us on:</h3>
          <div class="flex space-x-4">
            <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
              <i class="fab fa-facebook text-gray-400 hover:text-white"></i>
            </a>
            <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
              <i class="fab fa-instagram text-gray-400 hover:text-white"></i>
            </a>
            <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
              <i class="fab fa-twitter text-gray-400 hover:text-white"></i>
            </a>
          </div>
        </div>
      </div>
      <div class="mt-8 border-t border-gray-700 pt-4 text-center text-gray-500">
        &copy; {{ new Date().getFullYear() }} Pustakalaya — All Rights Reserved
      </div>
    </footer>
  </div>
</template>

<script>
import NavBar from "@/components/NavBar.vue";

export default {
  name: 'HomePage',
  components: { NavBar },
  data() {
    return {
      newsletter: { email: '' },
      featuredBooks: [
        { id: 1, title: 'The Silent Patient', author: 'Alex Michaelides', price: 550, image: '/assets/books/book1.jpg' },
        { id: 2, title: 'Atomic Habits', author: 'James Clear', price: 450, image: '/assets/books/book2.jpg' },
        { id: 3, title: 'The Power of Now', author: 'Eckhart Tolle', price: 400, image: '/assets/books/book3.jpg' },
        { id: 4, title: 'Sapiens', author: 'Yuval Noah Harari', price: 600, image: '/assets/books/book4.jpg' }
      ],
      categories: [
        { id: 1, name: 'Fiction', slug: 'fiction', image: '/assets/categories/fiction.jpg' },
        { id: 2, name: 'Non-Fiction', slug: 'non-fiction', image: '/assets/categories/non-fiction.jpg' },
        { id: 3, name: 'Self-Help', slug: 'self-help', image: '/assets/categories/self-help.jpg' },
        { id: 4, name: 'Biography', slug: 'biography', image: '/assets/categories/biography.jpg' }
      ]
    };
  },
  methods: {
    addToCart(bookId) {
      console.log(`Book ${bookId} added to cart`);
    },
    addToWishlist(bookId) {
      console.log(`Book ${bookId} added to wishlist`);
    },
    subscribeNewsletter() {
      console.log(`Subscribed with email: ${this.newsletter.email}`);
      this.newsletter.email = '';
    }
  }
};
</script>
